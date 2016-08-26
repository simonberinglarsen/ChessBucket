using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DemoSite.Data;
using DemoSite.Models;
using DemoSite.Services;

namespace DemoSite.Models
{
    public class QueueProcessor : IJob, IDisposable
    {
        private const int TimeOutLimitInMinutes = 15;
        private const int ConcurrentJobsAllowed = 2;
        private ApplicationDbContext _context;
        private BatchQueueItem _currentItem;

        public QueueProcessor()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(Startup.ConnectionString);
            _context = new ApplicationDbContext(optionsBuilder.Options);
        }

        public void Run()
        {
            // if already processing then stop
            var itemsProcessing = _context.BatchQueue.Where(x => x.QState == QState.Processing).ToList();
            // remove
            bool rerun = false;
            foreach (var itemProcessing in itemsProcessing)
            {
                // detach and reload to avoid caching issues (getting fresh data if we rerun)
                _context.Entry(itemProcessing).State = EntityState.Detached;
                // timed out?
                if ((DateTime.Now - itemProcessing.ModifiedAt).TotalMinutes > TimeOutLimitInMinutes)
                {
                    itemProcessing.QState = QState.Error;
                    itemProcessing.ModifiedAt = DateTime.Now;
                    itemProcessing.Error = "Timeout";
                    _context.SaveChanges();
                    rerun = true;
                }
            }
            if (rerun)
            {
                // rerun if job was timed out
                Run();
                return;
            }
            if (itemsProcessing.Count() >= ConcurrentJobsAllowed)
            {
                // a job is already running
                return;
            }

            // take next item and mark as processing
            _currentItem = _context.BatchQueue.OrderBy(x => x.CreatedAt).FirstOrDefault(x => x.QState == QState.Pending);
            if (_currentItem == null) return;
            _currentItem.QState = QState.Processing;
            _currentItem.ModifiedAt = DateTime.Now;
            _currentItem.ProcessingAt = DateTime.Now;
            _context.SaveChanges();

            // get referenced object
            int gameId = int.Parse(_currentItem.Reference);
            var comp = _context.Games.SingleOrDefault(x => x.Id == gameId);
            if (comp == null)
            {
                // game not found
                _currentItem.QState = QState.Error;
                _currentItem.ModifiedAt = DateTime.Now;
                _context.SaveChanges();
                return;
            }

            // detach objects from state tracker (enables direct database changes to affect dicision made in the job..)
            _context.Entry(comp).State = EntityState.Detached;
            _context.Entry(_currentItem).State = EntityState.Detached;

            // Start processing
            try
            {
                // analyze and update state
                Game gd = Compressor.Decompress(comp);
                Analyze analyze = new Analyze(20, this);
                var analysisResult = analyze.Game(gd.MovesLan);
                gd.AnalyzedMoves = analysisResult;
                gd.AnalysisInfo = analyze.Info;
                gd.AnalysisState = AnalysisState.Done;
                comp = Compressor.Compress(gd);

                // update database with new states and results
                _context.Entry(_currentItem).State = EntityState.Detached;
                _currentItem = _context.BatchQueue.Single(i => i.Id == _currentItem.Id);
                _currentItem.QState = QState.Done;
                _currentItem.ModifiedAt = DateTime.Now;
                _context.Games.Attach(comp);
                _context.Entry(comp).Property(x => x.CompressedData).IsModified = true;
                _context.Entry(comp).Property(x => x.AnalysisInfo).IsModified = true;
                _context.Entry(comp).Property(x => x.AnalysisState).IsModified = true;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _context.Entry(_currentItem).State = EntityState.Detached;
                _currentItem = _context.BatchQueue.Single(i => i.Id == _currentItem.Id);
                _currentItem.QState = QState.Error;
                _currentItem.ModifiedAt = DateTime.Now;
                _currentItem.Error = ex.ToString();
                _context.SaveChanges();
                throw;
            }
        }

        public void Ping(string logLine)
        {
            // detach and reload to avoid caching = fresh data
            _context.Entry(_currentItem).State = EntityState.Detached;
            _currentItem = _context.BatchQueue.Single(i => i.Id == _currentItem.Id);
            _currentItem.Info = logLine;
            _currentItem.ModifiedAt = DateTime.Now;
            _context.SaveChanges();
        }

        public bool IsProcessing
        {
            get
            {
                // detach and reload to avoid caching = fresh data
                _context.Entry(_currentItem).State = EntityState.Detached;
                _currentItem = _context.BatchQueue.Single(i => i.Id == _currentItem.Id);
                return _currentItem.QState == QState.Processing;
            }
        }

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }


    public interface IJob
    {
        void Ping(string logLine);
        bool IsProcessing { get; }
    }
}

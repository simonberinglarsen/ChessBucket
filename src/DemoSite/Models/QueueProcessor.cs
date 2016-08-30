using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessBucket.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChessBucket.Models;
using ChessBucket.Services;

namespace ChessBucket.Models
{
    public class QueueProcessor : IJob
    {
        private const int TimeOutLimitInMinutes = 15;
        private const int ConcurrentJobsAllowed = 2;
        private int _id;

        public QueueProcessor()
        {
        }

        private ApplicationDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(Startup.ConnectionString);
            return new ApplicationDbContext(optionsBuilder.Options);

        }

        public void Run()
        {
            if (CleanupJobsThatTimedOut())
            {
                Run();
                return;
            }
            if (TooManyConcurrentJobs())
                _id = NextItemIdToForceProcess();
            else
                _id = NextItemIdToProcess();
            if (_id == -1)
                return;
            Game game = LoadReferencedGame();
            if (game == null)
                return;
            // Start processing
            try
            {
                // analyze and update game
                AnalyzeGame(game);
                UpdateGame(game);
            }
            catch (CancelJobException jobEx)
            {
                // cancel item
                using (var context = CreateContext())
                {
                    var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                    queueItem.QState = QState.Cancel;
                    queueItem.ModifiedAt = DateTime.Now;
                    queueItem.Info = jobEx.ToString();
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // mark item as failed
                using (var context = CreateContext())
                {
                    var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                    queueItem.QState = QState.Error;
                    queueItem.ModifiedAt = DateTime.Now;
                    queueItem.Error = ex.ToString();
                    context.SaveChanges();
                }
            }
        }

        private int NextItemIdToForceProcess()
        {
            using (var context = CreateContext())
            {
                // take next item and mark as processing
                var nextItem = context.BatchQueue.OrderBy(x => x.CreatedAt).FirstOrDefault(x => x.QState == QState.ForceProcessing);
                if (nextItem == null)
                    return -1;
                nextItem.QState = QState.Processing;
                nextItem.ModifiedAt = DateTime.Now;
                nextItem.ProcessingAt = DateTime.Now;
                context.SaveChanges();
                return nextItem.Id;
            }
        }

        private void AnalyzeGame(Game game)
        {
            Analyze analyze = new Analyze(20, this);
            var analysisResult = analyze.Game(game.MovesLan);
            game.AnalyzedMoves = analysisResult;
            game.AnalysisInfo = analyze.Info;
            game.AnalysisState = AnalysisState.Done;
        }

        private void UpdateGame(Game gd)
        {
            // update database with new states and results
            using (var context = CreateContext())
            {
                var comp = Compressor.Compress(gd);
                var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                queueItem.QState = QState.Done;
                queueItem.ModifiedAt = DateTime.Now;
                context.Games.Attach(comp);
                context.Entry(comp).Property(x => x.CompressedData).IsModified = true;
                context.Entry(comp).Property(x => x.AnalysisInfo).IsModified = true;
                context.Entry(comp).Property(x => x.AnalysisState).IsModified = true;
                context.SaveChanges();
            }
        }

        private Game LoadReferencedGame()
        {
            // get referenced object
            using (var context = CreateContext())
            {
                var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                int gameId = int.Parse(queueItem.Reference);
                var comp = context.Games.SingleOrDefault(x => x.Id == gameId);
                if (comp == null)
                {
                    // game not found
                    queueItem.QState = QState.Error;
                    queueItem.ModifiedAt = DateTime.Now;
                    context.SaveChanges();
                    return null;
                }
                return Compressor.Decompress(comp);
            }
        }

        private int NextItemIdToProcess()
        {
            using (var context = CreateContext())
            {
                // take next item and mark as processing
                var nextItem = context.BatchQueue.OrderBy(x => x.CreatedAt).FirstOrDefault(x => x.QState == QState.Pending);
                if (nextItem == null)
                    return -1;
                nextItem.QState = QState.Processing;
                nextItem.ModifiedAt = DateTime.Now;
                nextItem.ProcessingAt = DateTime.Now;
                context.SaveChanges();
                return nextItem.Id;
            }
        }

        private bool TooManyConcurrentJobs()
        {
            using (var context = CreateContext())
            {
                var itemsProcessing = context.BatchQueue.Where(x => x.QState == QState.Processing).ToList();

                if (itemsProcessing.Count() >= ConcurrentJobsAllowed)
                {
                    // a job is already running
                    return true;
                }
                return false;
            }
        }

        private bool CleanupJobsThatTimedOut()
        {
            // if already processing then stop
            using (var context = CreateContext())
            {
                var itemsProcessing = context.BatchQueue.Where(x => x.QState == QState.Processing).ToList();
                // remove
                bool atLeastOneTimeout = false;
                foreach (var itemProcessing in itemsProcessing)
                {
                    // detach and reload to avoid caching issues (getting fresh data if we rerun)
                    context.Entry(itemProcessing).State = EntityState.Detached;
                    // timed out?
                    if ((DateTime.Now - itemProcessing.ModifiedAt).TotalMinutes > TimeOutLimitInMinutes)
                    {
                        itemProcessing.QState = QState.Error;
                        itemProcessing.ModifiedAt = DateTime.Now;
                        itemProcessing.Error = "Timeout";
                        context.SaveChanges();
                        atLeastOneTimeout = true;
                    }
                    if (atLeastOneTimeout) return true;
                }
                return false;
            }
        }

        public void Ping(string logLine)
        {
            using (var context = CreateContext())
            {
                var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                queueItem.Info = logLine;
                queueItem.ModifiedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public QState QState
        {
            get
            {
                using (var context = CreateContext())
                {
                    var queueItem = context.BatchQueue.Single(i => i.Id == _id);
                    return queueItem.QState;
                }
            }
        }
    }


    public interface IJob
    {
        void Ping(string logLine);
        QState QState { get; }
    }
}

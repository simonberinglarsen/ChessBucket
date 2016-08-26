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
    public class QueueProcessor
    {

        public QueueProcessor()
        {

        }

        public void Run()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(Startup.ConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {

                // if already processing then stop
                if (context.BatchQueue.Any(x => x.QState == QState.Processing))
                    return;

                // take next item and mark as processing
                var nextItem =
                    context.BatchQueue.OrderBy(x => x.CreatedAt).FirstOrDefault(x => x.QState == QState.Pending);
                if (nextItem == null)
                    return;
                nextItem.QState = QState.Processing;
                nextItem.ModifiedAt = DateTime.Now;
                nextItem.ProcessingAt = DateTime.Now;
                context.SaveChanges();

                // get referenced object
                int gameId = int.Parse(nextItem.Reference);
                var comp = context.Games.SingleOrDefault(x => x.Id == gameId);
                if (comp == null)
                {
                    nextItem.QState = QState.Error;
                    nextItem.ModifiedAt = DateTime.Now;
                    context.SaveChanges();
                    return;
                }
                context.Entry(comp).State = EntityState.Detached;

                // Start processing
                try
                {

                    Game gd = Compressor.Decompress(comp);
                    Analyze analyze = new Analyze(20);
                    var analysisResult = analyze.Game(gd.MovesLan);

                    // update database with new states and results
                    gd.AnalyzedMoves = analysisResult;
                    gd.AnalysisInfo = analyze.Info;
                    gd.AnalysisState = AnalysisState.Done;
                    comp = Compressor.Compress(gd);
                    nextItem.QState = QState.Done;
                    nextItem.ModifiedAt = DateTime.Now;
                    context.Games.Attach(comp);
                    context.Entry(comp).Property(x => x.CompressedData).IsModified = true;
                    context.Entry(comp).Property(x => x.AnalysisInfo).IsModified = true;
                    context.Entry(comp).Property(x => x.AnalysisState).IsModified = true;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    nextItem.QState = QState.Error;
                    nextItem.ModifiedAt = DateTime.Now;
                    nextItem.Error = ex.ToString();
                    context.SaveChanges();
                    throw;
                }
            }
        }
    }
}

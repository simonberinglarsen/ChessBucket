using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DemoSite.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DemoSite.Models.GameViewModels;
using System.Text;
using DemoSite.Data;
using Microsoft.EntityFrameworkCore;

namespace DemoSite.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }
        public IActionResult Search()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Show(int id)
        {
            return View(new ShowGameViewModel() { Id = id });
        }

        [HttpGet]
        public string ParseGame(string pgnText)
        {
            StringBuilder allErrors = new StringBuilder();
            int? newGameId = null;
            try
            {
                // parse game
                pgnText = pgnText ?? "";
                PgnParser parser = new PgnParser();
                parser.LoadPgn(pgnText);
                foreach (var game in parser.PgnGames)
                {
                    StringBuilder gameErrors = new StringBuilder();
                    // store game in database
                    Game gd = new Game();
                    gd.AnalysisState = AnalysisState.Pending;
                    gd.Event = game.Headers.ContainsKey("EVENT") ? game.Headers["EVENT"] : null;
                    gd.Site = game.Headers.ContainsKey("SITE") ? game.Headers["SITE"] : null;
                    gd.Date = game.Headers.ContainsKey("DATE") ? game.Headers["DATE"] : null;
                    gd.Round = game.Headers.ContainsKey("ROUND") ? game.Headers["ROUND"] : null;
                    gd.White = game.Headers.ContainsKey("WHITE") ? game.Headers["WHITE"] : null;
                    gd.Black = game.Headers.ContainsKey("BLACK") ? game.Headers["BLACK"] : null;
                    gd.Result = game.Headers.ContainsKey("RESULT") ? game.Headers["RESULT"] : null;
                    if (gd.Event == null)
                        gameErrors.AppendLine("event header missing");
                    if (gd.Site == null)
                        gameErrors.AppendLine("site header missing");
                    if (gd.Date == null)
                        gameErrors.AppendLine("date header missing");
                    if (gd.Round == null)
                        gameErrors.AppendLine("round header missing");
                    if (gd.White == null)
                        gameErrors.AppendLine("white header missing");
                    if (gd.Black == null)
                        gameErrors.AppendLine("black header missing");
                    if (gd.Result == null)
                        gameErrors.AppendLine("result header missing");
                    if (gameErrors.Length > 0)
                    {
                        allErrors.AppendLine(gameErrors.ToString());
                        continue;
                    }
                    gd.MovesLan = game.MovesLan;
                    gd.MovesSan = game.MovesSan;
                    // empty analysis (to be able to show game..)
                    List<AnalyzedMove> analyzedMoves = new List<AnalyzedMove>();
                    Board b = new Board();
                    b.StartPosition();
                    foreach (var moveLan in gd.MovesLan)
                    {
                        Move gameMove = Move.FromLan(moveLan);
                        AnalyzedMove analyzedMove = new AnalyzedMove();
                        analyzedMove.IsWhite = b.WhitesTurn;
                        analyzedMove.BestMove = new EvaluatedMove()
                        {
                            Value = 0,
                            MoveLan = moveLan,
                            MoveSan = b.ToSan(moveLan),
                            DeltaToBest = 0,
                            PrincipalVariation = null
                        };
                        b.DoMove(gameMove);
                        analyzedMoves.Add(analyzedMove);
                    }
                    gd.AnalyzedMoves = analyzedMoves.ToArray();
                    var comp = Compressor.Compress(gd);
                    // save new game
                    _context.Games.Add(comp);
                    _context.SaveChanges();

                    // add entry in batchqueue (schedule a game analysis)
                    DateTime now = DateTime.Now;
                    _context.BatchQueue.Add(new BatchQueueItem()
                    {
                        QState = QState.Pending,
                        QType = QType.GameAnalysis,
                        Reference = comp.Id.ToString(),
                        CreatedAt = now,
                        ModifiedAt = now
                    });
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                allErrors.AppendLine(ex.Message);
            }
            // return result?
            return JsonConvert.SerializeObject(new { Errors = allErrors.ToString() });
        }



        [HttpGet]
        public string LoadGame(int id)
        {

            GameCompressed comp = _context.Games.FirstOrDefault(g => g.Id == id);
            Game gd = Compressor.Decompress(comp);

            // map gamedata to viewmodel
            List<string> moveList = gd.MovesSan.ToList();
            AnalyzedGameViewModel vm = new AnalyzedGameViewModel(gd) { AnalyzedMoves = new List<AnalyzedMoveViewModel>() };

            if (gd.AnalyzedMoves != null)
            {
                Board b = new Board();
                b.StartPosition();
                for (int i = 0; i < gd.AnalyzedMoves.Length; i++)
                {
                    vm.AnalyzedMoves.Add(AnalyzedMoveViewModel.MapFrom(gd.AnalyzedMoves[i], b));
                    vm.AnalyzedMoves.Last().Description = (gd.AnalyzedMoves[i].IsWhite ? (i / 2 + 1) + "." : "") + moveList[i];
                    b.DoMove(Move.FromLan(gd.AnalyzedMoves[i].ActualMove == null ? gd.AnalyzedMoves[i].BestMove.MoveLan : gd.AnalyzedMoves[i].ActualMove.MoveLan));
                }
            }
            return JsonConvert.SerializeObject(vm);
        }

      

        public string SearchGames(string searchText, int page)
        {
            var query = _context.Games;
            int maxPage = (query.Count() - 1) / PageSize;
            if (page >= maxPage)
                page = maxPage;
            else if (page < 0)
                page = 0;
            var games = query.OrderBy(x => x.Event).Skip(PageSize * page).Take(PageSize);
            GameSearchViewModel vm = new GameSearchViewModel() { SearchResults = new List<SearchResultViewModel>() };
            vm.CurrentPage = page;
            vm.SearchKeyword = searchText;
            foreach (var game in games)
            {
                vm.SearchResults.Add(new SearchResultViewModel()
                {
                    GameId = game.Id,
                    Event = game.Event,
                    Site = game.Site,
                    Date = game.Date,
                    Round = game.Round,
                    White = game.White,
                    Black = game.Black,
                    Result = game.Result,
                    AnalysisState = game.AnalysisState.ToString()
                });
            }
            return JsonConvert.SerializeObject(vm);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ConsoleApp1;
using DemoSite.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DemoSite.Models.GameViewModels;
using System.Text;

namespace DemoSite.Controllers
{
    public class GameController : Controller
    {
        private const int PageSize = 10;
        private const int analysisDepth = 20;
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
            StringBuilder errors = new StringBuilder();
            try
            {
                MockedDatabase.Instance.Load();

                // parse game
                pgnText = pgnText ?? "";
                PgnParser parser = new PgnParser();
                parser.LoadPgn(pgnText);

                foreach (var game in parser.PgnGames)
                {
                    // store game in database
                    GameData gd = new GameData();
                    gd.AnalysisState = AnalysisState.Pending;
                    if (MockedDatabase.Instance.Games.Count() == 0)
                        gd.Id = 1;
                    else
                        gd.Id = MockedDatabase.Instance.Games.Max(x => x.Id) + 1;
                    gd.Event = game.Headers.ContainsKey("EVENT") ? game.Headers["EVENT"] : null;
                    gd.Site = game.Headers.ContainsKey("SITE") ? game.Headers["SITE"] : null;
                    gd.Date = game.Headers.ContainsKey("DATE") ? game.Headers["DATE"] : null;
                    gd.Round = game.Headers.ContainsKey("ROUND") ? game.Headers["ROUND"] : null;
                    gd.White = game.Headers.ContainsKey("WHITE") ? game.Headers["WHITE"] : null;
                    gd.Black = game.Headers.ContainsKey("BLACK") ? game.Headers["BLACK"] : null;
                    gd.Result = game.Headers.ContainsKey("RESULT") ? game.Headers["RESULT"] : null;
                    if (gd.Event == null)
                        errors.AppendLine("event header missing");
                    if (gd.Site == null)
                        errors.AppendLine("site header missing");
                    if (gd.Date == null)
                        errors.AppendLine("date header missing");
                    if (gd.Round == null)
                        errors.AppendLine("round header missing");
                    if (gd.White == null)
                        errors.AppendLine("white header missing");
                    if (gd.Black == null)
                        errors.AppendLine("black header missing");
                    if (gd.Result == null)
                        errors.AppendLine("result header missing");
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
                        var allmoves = b.GenerateMoves();
                        b.PopulateSan(allmoves);
                        analyzedMove.BestMove = new EvaluatedMove()
                        {
                            Value = 0,
                            MoveLan = moveLan,
                            MoveSan = allmoves.Single(x => x.Lan == moveLan).San,
                            DeltaToBest = 0,
                            PrincipalVariation = null
                        };
                        b.DoMove(gameMove);
                        analyzedMoves.Add(analyzedMove);
                    }
                    gd.AnalyzedMoves = analyzedMoves.ToArray();
                    MockedDatabase.Instance.Games.Add(gd);
                }
                if (errors.Length == 0)
                {
                    MockedDatabase.Instance.Save();
                }
            }
            catch(Exception ex)
            {
                errors.AppendLine(ex.Message);
            }
            // return result?
            return JsonConvert.SerializeObject(new { Errors = errors.ToString() });
        }



        [HttpGet]
        public string LoadGame(int id)
        {
            MockedDatabase.Instance.Load();
            GameData gd = MockedDatabase.Instance.Games.FirstOrDefault(g => g.Id == id);

            // map gamedata to viewmodel
            List<string> moveList = gd.MovesSan.ToList();
            AnalyzedGameViewModel vm = new AnalyzedGameViewModel { AnalyzedMoves = new List<AnalyzedMoveViewModel>() };
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

        [HttpGet]
        public void AnalyzeGame()
        {
            MockedDatabase.Instance.Load();
            GameData gd = MockedDatabase.Instance.Games.FirstOrDefault(g => g.AnalysisState == AnalysisState.Pending);
            if (gd == null) return;
            gd.AnalysisState = AnalysisState.Started;
            MockedDatabase.Instance.Save();

            gd.AnalyzedMoves = Analyze.Game(gd.MovesLan, analysisDepth);
            gd.AnalysisState = AnalysisState.Done;
            MockedDatabase.Instance.Save();
        }

        [HttpGet]
        public void AnalyzeAll()
        {
            while (true)
            {
                MockedDatabase.Instance.Load();
                GameData gd = MockedDatabase.Instance.Games.FirstOrDefault(g => g.AnalysisState == AnalysisState.Pending);
                if (gd == null) return;
                gd.AnalysisState = AnalysisState.Started;
                MockedDatabase.Instance.Save();

                gd.AnalyzedMoves = Analyze.Game(gd.MovesLan, analysisDepth);
                gd.AnalysisState = AnalysisState.Done;
                MockedDatabase.Instance.Save();
            }
        }

        public string SearchGames(string searchText, int page)
        {
            MockedDatabase.Instance.Load();
            var query = MockedDatabase.Instance.Games.Where(x => string.IsNullOrWhiteSpace(searchText));
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

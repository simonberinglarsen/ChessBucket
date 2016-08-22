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

namespace DemoSite.Controllers
{
    public class GameController : Controller
    {
        private const int PageSize = 5;
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
            MockedDatabase.Instance.Load();

            // parse game
            PgnParser parser = new PgnParser();
            //parser.LoadPgn(System.IO.File.ReadAllText(@"C:\Users\bc0618\Desktop\simon\partier\Candidates1962.pgn.txt"));
            //parser.LoadPgn(System.IO.File.ReadAllText(@"C:\Users\bc0618\Desktop\simon\partier\IMG_20160813_184158.pgn"));
            parser.LoadPgn(pgnText);

            // store game in database
            GameData gd = new GameData();
            gd.Analyzed = false;
            if (MockedDatabase.Instance.Games.Count() == 0)
                gd.Id = 1;
            else
                gd.Id = MockedDatabase.Instance.Games.Max(x => x.Id) + 1;
            gd.MovesLan = parser.MovesLan;
            gd.MovesSan = parser.MovesSan;
            MockedDatabase.Instance.Games.Add(gd);
            MockedDatabase.Instance.Save();

            // return result?
            return JsonConvert.SerializeObject(new { });
        }



        [HttpGet]
        public string LoadGame(int id)
        {
            MockedDatabase.Instance.Load();
            GameData gd = MockedDatabase.Instance.Games.FirstOrDefault(g => g.Id == id);

            // map gamedata to viewmodel
            List<string> moveList = gd.MovesSan.ToList();
            AnalyzedGameViewModel vm = new AnalyzedGameViewModel { AnalyzedMoves = new List<AnalyzedMove>() };
            if (gd.AnalyzedMoves != null)
            {
                for (int i = 0; i < gd.AnalyzedMoves.Length; i++)
                {
                    gd.AnalyzedMoves[i].Description = (gd.AnalyzedMoves[i].IsWhite ? (i / 2 + 1) + "." : "") + moveList[i];
                    vm.AnalyzedMoves.Add(gd.AnalyzedMoves[i]);
                }
            }
            return JsonConvert.SerializeObject(vm);
        }

        [HttpGet]
        public void AnalyzeGame()
        {
            MockedDatabase.Instance.Load();
            GameData gd = MockedDatabase.Instance.Games.FirstOrDefault(g => !g.Analyzed);
            if (gd == null) return;

            gd.AnalyzedMoves = Analyze.Game(gd.MovesLan.ToList());
            gd.Analyzed = true;
            MockedDatabase.Instance.Save();
        }

        public string SearchGames(string searchText, int page)
        {
            MockedDatabase.Instance.Load();
            var query = MockedDatabase.Instance.Games.Where(x => string.IsNullOrWhiteSpace(searchText) || x.Tag == searchText);
            int maxPage = (query.Count()-1) / PageSize;
            if (page >= maxPage)
                page = maxPage;
            else if (page < 0)
                page = 0;
            var games = query.OrderBy(x => x.Tag).Skip(PageSize * page).Take(PageSize);
            GameSearchViewModel vm = new GameSearchViewModel() { SearchResults = new List<SearchResultViewModel>() };
            vm.CurrentPage = page;
            vm.SearchKeyword = searchText;
            foreach (var game in games)
            {
                vm.SearchResults.Add(new SearchResultViewModel()
                {
                    GameId = game.Id,
                    Tournament = "Testing"
                });
            }
            return JsonConvert.SerializeObject(vm);
        }
      
    }
}

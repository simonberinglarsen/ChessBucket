using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ConsoleApp1;
using DemoSite.Models;
using DemoSite.Models.BoardViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DemoSite.Controllers
{
    public class BoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Movegen()
        {
            return View();
        }

        [HttpGet]
        public string RandomGame()
        {

            PgnParser parser = new PgnParser();
            //parser.LoadPgn(System.IO.File.ReadAllText(@"C:\Users\bc0618\Desktop\simon\partier\Candidates1962.pgn.txt"));
            parser.LoadPgn(System.IO.File.ReadAllText(@"C:\Users\bc0618\Desktop\simon\partier\IMG_20160813_184222.pgn"));
            List<string> moveList = parser.MovesSan.ToList();
            var analyzedMoves = Analyze.Game(parser.MovesLan.ToList());

            // map to viewmodel
            AnalyzedGameViewModel vm = new AnalyzedGameViewModel { AnalyzedMoves = new List<AnalyzedMove>() };
            for (int i = 0; i < analyzedMoves.Count; i++)
            {
                analyzedMoves[i].Description = (i / 2 + 1) + ". " + (!analyzedMoves[i].IsWhite ? "-, " : "") + moveList[i];
                vm.AnalyzedMoves.Add(analyzedMoves[i]);
            }

            return JsonConvert.SerializeObject(vm);


        }
    }
}

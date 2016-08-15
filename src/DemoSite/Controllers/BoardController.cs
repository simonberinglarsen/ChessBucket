using System;
using System.Collections.Generic;
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
            try
            {


                PgnParser parser = new PgnParser();
                parser.LoadPgn(System.IO.File.ReadAllText(@"C:\Users\bc0618\Desktop\simon\partier\Candidates1962.pgn.txt"));

                List<string> moveList = parser.MovesSan.ToList();
                var evaluatedMoves = Analyze.Game(parser.MovesLan.ToList());

                AnalyzedGameViewModel vm = new AnalyzedGameViewModel();
                vm.EvaluatedMoves = new List<EvaluatedMoveWithAlternatives>();
                for (int i = 0; i < evaluatedMoves.Count; i++)
                {
                    evaluatedMoves[i].Move = (i / 2 + 1) + ". " + (!evaluatedMoves[i].IsWhite ? "-, " : "") + moveList[i];
                    vm.EvaluatedMoves.Add(evaluatedMoves[i]);
                }

                return JsonConvert.SerializeObject(vm);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

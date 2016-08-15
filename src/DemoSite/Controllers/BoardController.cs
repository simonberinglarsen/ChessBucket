using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
            GameGenerator game = new GameGenerator();
            List<string> moveList;
            //moveList = game.Generate();
            moveList = game.GeneratePalleVsSimon();

            var evaluatedMoves = Analyze.Game(moveList);

            AnalyzedGameViewModel vm = new AnalyzedGameViewModel();
            vm.EvaluatedMoves = new List<ColoredEvaluatedMove>();
            for (int i = 0; i < evaluatedMoves.Count; i++)
            {
                if (i % 2 == 0)
                {
                    vm.EvaluatedMoves.Add(new ColoredEvaluatedMove() { MoveNumber = 1 + i / 2 });
                    vm.EvaluatedMoves[i / 2].White = evaluatedMoves[i];
                }
                else
                {
                    vm.EvaluatedMoves[i / 2].Black = evaluatedMoves[i];
                }
            }

            return JsonConvert.SerializeObject(vm);
        }
    }
}

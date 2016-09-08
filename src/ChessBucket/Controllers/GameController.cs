using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using ChessBucket.Data;
using ChessBucket.Models;
using ChessBucket.Models.GameViewModels;
using Microsoft.EntityFrameworkCore;

namespace ChessBucket.Controllers
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
        public string GetTags(int gameId)
        {
            var tags =
                _context.Games.Where(g => g.Id == gameId)
                    .Include(g => g.Tags)
                    .ThenInclude(x => x.Tag).SelectMany(t => t.Tags.Select(q => q.Tag.Name)).ToList();
            return JsonConvert.SerializeObject(tags);
        }


        [HttpGet]
        [ResponseCache(Duration = 0)]
        public string GetAllTags(string filter)
        {
            var tags = _context.Tags.Where(t => t.Name.ToLower().Contains(filter.ToLower())).Select(t => t.Name).ToList();
            return JsonConvert.SerializeObject(tags);
        }

        [HttpPost]
        public string UpdateTags([FromBody]TagData input)
        {
            try
            {
                // new tags
                List<Tag> knownTags =
                    _context.Tags.Where(t => input.tags.Contains(t.Name, StringComparer.OrdinalIgnoreCase)).ToList();
                var newTags =
                    input.tags.Where(
                        inputTag =>
                            !knownTags.Exists(
                                knownTag =>
                                    string.Compare(knownTag.Name, inputTag, StringComparison.OrdinalIgnoreCase) == 0))
                        .Select(t => new Tag() { Id = 0, Name = t.ToLower() })
                        .ToList();
                // add new tags
                _context.Tags.AddRange(newTags);
                _context.SaveChanges();
                // remove old tags for game
                _context.GameTags.RemoveRange(_context.GameTags.Where(g => g.Game.Id == input.gameId));
                _context.SaveChanges();
                // link input tags to game
                GameCompressed existingGame = new GameCompressed() { Id = input.gameId };
                _context.Entry(existingGame).State = EntityState.Unchanged;
                _context.GameTags.AddRange(knownTags.Select(t => new GameTag() { Tag = t, Game = existingGame }));
                _context.GameTags.AddRange(newTags.Select(t => new GameTag() { Tag = t, Game = existingGame }));
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }


            return "";
        }

        [HttpPost]
        public string Post([FromBody]PgnData data)
        {
            string pgnText = data.pgnText;
            StringBuilder allErrors = new StringBuilder();
            try
            {
                // parse game
                pgnText = pgnText ?? "";
                PgnParser parser = new PgnParser();
                parser.LoadPgn(pgnText);
                for (int pass = 1; pass <= 2; pass++)
                {
                    if (pass == 2)
                    {
                        if (allErrors.Length > 0)
                            throw new Exception("No games saved. Please correct the errors in the pgn file..");
                    }

                    foreach (var game in parser.PgnGames)
                    {
                        string currentGame = game.HeaderString() + "...";
                        StringBuilder gameErrors = new StringBuilder();
                        // store game in database
                        Game gd = new Game();
                        gd.AnalysisState = AnalysisState.Pending;
                        gd.Event = game.Headers.ContainsKey("EVENT") ? game.Headers["EVENT"] : null;
                        gd.Site = game.Headers.ContainsKey("SITE") ? game.Headers["SITE"] : null;
                        gd.Date = game.Headers.ContainsKey("DATE") ? game.Headers["DATE"] : null;
                        gd.White = game.Headers.ContainsKey("WHITE") ? game.Headers["WHITE"] : null;
                        gd.Black = game.Headers.ContainsKey("BLACK") ? game.Headers["BLACK"] : null;
                        gd.WhiteElo = game.Headers.ContainsKey("WHITEELO") ? game.Headers["WHITEELO"] : null;
                        gd.BlackElo = game.Headers.ContainsKey("BLACKELO") ? game.Headers["BLACKELO"] : null;
                        gd.Result = game.Headers.ContainsKey("RESULT") ? game.Headers["RESULT"] : null;
                        if (gd.Event == null)
                            gameErrors.AppendLine("Event-header missing: " + currentGame);
                        if (gd.Site == null)
                            gameErrors.AppendLine("Site-header missing: " + currentGame);
                        if (gd.Date == null)
                            gameErrors.AppendLine("Date-header missing: " + currentGame);
                        if (gd.White == null)
                            gameErrors.AppendLine("White-header missing: " + currentGame);
                        if (gd.Black == null)
                            gameErrors.AppendLine("Black-header missing: " + currentGame);
                        if (gd.WhiteElo == null)
                            gameErrors.AppendLine("WhiteElo-header missing: " + currentGame);
                        if (gd.BlackElo == null)
                            gameErrors.AppendLine("BlackElo-header missing: " + currentGame);
                        if (gd.Result == null)
                            gameErrors.AppendLine("Result-header missing: " + currentGame);
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
                        if (pass == 2)
                        {
                            // only save stuff if we reached 2nd pass
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
                    White = game.White,
                    Black = game.Black,
                    WhiteElo = game.WhiteElo,
                    BlackElo = game.BlackElo,
                    Result = game.Result,
                    AnalysisState = game.AnalysisState.ToString()
                });
            }
            return JsonConvert.SerializeObject(vm);
        }
        public class PgnData
        {
            public string pgnText { get; set; }
        }

        public class TagData
        {
            public int gameId { get; set; }
            public string[] tags { get; set; }
        }

    }
}

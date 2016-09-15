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
    public class OpeningController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OpeningController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BuildTree()
        {
            // clear current opening tree
            _context.Database.ExecuteSqlCommand("truncate table \"TreeNodeTransitions\" CASCADE");
            _context.Database.ExecuteSqlCommand("truncate table \"TreeNodes\" CASCADE");
            _context.Database.ExecuteSqlCommand("ALTER SEQUENCE \"TreeNodeTransitions_Id_seq\" RESTART WITH 1");
            _context.Database.ExecuteSqlCommand("ALTER SEQUENCE \"TreeNodes_Id_seq\" RESTART WITH 1");
            _context.SaveChanges();

            //loop through all games
            List<TreeNode> contextTreeNodes = new List<TreeNode>();
            List<TreeNodeTransition> contextTreeNodeTransitions = new List<TreeNodeTransition>();

            // traverse game and add to tree (given hardcoded depth)
            int openingGraphDepth = 15;
            int[] gameIds = _context.Games.Select(g => g.Id).ToArray();
            foreach (var gameId in gameIds)
            {
                var compressedGame = _context.Games.First(g => g.Id == gameId);
                var game = Compressor.Decompress(compressedGame);
                Board b = new Board();
                b.StartPosition();
                TreeNode parent = null;
                for (int i = 0; i < game.MovesLan.Length && i < openingGraphDepth; i++)
                {
                    string player = b.WhitesTurn ? game.White : game.Black;
                    b.DoMove(Move.FromLan(game.MovesLan[i]));
                    string fen = b.DumpFen();
                    string moveSan = game.MovesSan[i];
                    TreeNode childNode = new TreeNode()
                    {
                        Fen = fen,
                        Value = 0,
                    };
                    var existingChildNode = contextTreeNodes.FirstOrDefault(t => t.Fen == fen);
                    if (existingChildNode != null)
                        childNode = existingChildNode; 
                    else
                        contextTreeNodes.Add(childNode);
                    bool relationExists = contextTreeNodeTransitions.Any(t => t.Parent == parent && t.Child == childNode);
                    if (!relationExists)
                    {
                        TreeNodeTransition relation = new TreeNodeTransition()
                        {
                            Parent = parent,
                            Child = childNode,
                            San = moveSan,
                            Player = player

                        };
                        contextTreeNodeTransitions.Add(relation);
                    }
                    parent = childNode;
                }
            }
            _context.TreeNodes.AddRange(contextTreeNodes);
            _context.SaveChanges();
            _context.TreeNodeTransitions.AddRange(contextTreeNodeTransitions);
            _context.SaveChanges();

            return View();
        }

        [HttpGet]
        public string TransitionsByNodeId(int parentId)
        {
            TreeNodeTransition[] transitions;
            if (parentId == 0)
            {
                Board b = new Board();
                b.StartPosition();
                transitions = _context.TreeNodeTransitions.Include(x => x.Parent).Include(x => x.Child).Where(t => t.Parent == null).ToArray();
            }
            else
            {
                transitions = _context.TreeNodeTransitions.Include(x => x.Parent).Include(x => x.Child).Where(t => t.Parent != null && t.Parent.Id == parentId).ToArray();
            }
            return JsonConvert.SerializeObject(new
            {
                transitions
            });
        }
    }

}

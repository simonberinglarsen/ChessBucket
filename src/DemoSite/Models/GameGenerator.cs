using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DemoSite.Models.ManageViewModels;

namespace DemoSite.Models
{
    public class GameGenerator
    {
        public List<string> Generate()
        {
            Random r = new Random((int) (DateTime.Now.Ticks & 0xffffffff));
            Board b = new Board();
            List<string> moveList = new List<string>();
            b.StartPosition();
            bool done = false;
            for (int i = 0; i < 300; i++)
            {
                if (b.IsDraw || b.IsMate)
                    break;
                var moves = b.GenerateMoves();
                string beforeFen = b.DumpFen();
                List<Move> bestMoves = new List<Move>();
                int bestScore = 0;
                int adjustment = b.WhitesTurn ? 1 : -1;
                for (int j = 0; j < moves.Length; j++)
                {
                    b.DoMove(moves[j]);
                    int score = Analyze.MaterialScore(b);
                    int adjustedScore = score*adjustment;
                    if (adjustedScore > bestScore || bestMoves.Count == 0)
                    {
                        bestMoves.Clear();
                        bestMoves.Add(moves[j]);
                        bestScore = adjustedScore;
                    }
                    else if (adjustedScore == bestScore)
                    {
                        bestMoves.Add(moves[j]);
                    }
                    b.Setup(beforeFen);
                }
                string beforeFen2 = b.DumpFen();
                if (beforeFen2 != beforeFen)
                    throw new Exception();
                Move bestMove = bestMoves[r.Next(bestMoves.Count)];
                b.DoMove(bestMove);
                moveList.Add(bestMove.Text);
            }
            return moveList;
        }

        public List<string> GeneratePalleVsSimon()
        {
            string game = @"d2d4 g8f6 b1c3 d7d5 e2e4 d5e4 f2f3 e7e5 d4e5 d8d1 c3d1 f6d7 e5e6 f7e6 f3e4
f8d6 g1f3 e8g8 c1e3 b8c6 d1f2 d7e5 f1e2 c6b4 e1d2 f8d8 e2d3 b7b6 a2a3 b4d3
c2d3 c8a6 d2c2 e5f3 g2f3 d8f8 f2g4 f8f3 h1g1 h7h5 g4h6 g8h7 g1g2 f3e3 a1g1
a6d3 c2d2 e3e2";
            List<string> moves = new List<string>();
            moves.AddRange(game.Split(new char[] {' ', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries));
            return moves;
        }
    }
}
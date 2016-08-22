using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleApp1;
using Microsoft.AspNetCore.Razor.Tools.Internal;

namespace DemoSite.Models
{
    public class Analyze
    {
        public static AnalyzedMove[] Game(List<string> moveList)
        {
            int depth = 10;
            string beforeFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            using (var proxy = new SFProxy())
            {
                Board b = new Board();
                b.StartPosition();
                List<AnalyzedMove> analyzedMoves = new List<AnalyzedMove>();
                StringBuilder moveSequence = new StringBuilder();
                foreach (var moveText in moveList)
                {
                    if(moveText == "g2g4")
                    {
                        int k = 8;
                    }
                    System.Diagnostics.Debug.WriteLine(moveText);
                    Move gameMove = b.MoveFromText(moveText);
                    AnalyzedMove analyzedMove = new AnalyzedMove();
                    analyzedMove.IsWhite = b.WhitesTurn;
                    // sanity check ... to be removed
                    string fen = proxy.FenAfterMoves(moveSequence.ToString());
                    if (fen != beforeFen)
                    {
                        throw new Exception("last move failed! - >" + moveSequence.ToString());
                    }
                    // array of actual- and bestmove
                    analyzedMove.BestMove = proxy.Go(moveSequence.ToString(), depth);
                    if(analyzedMove.BestMove.MoveLan != moveText)
                        analyzedMove.ActualMove = proxy.Go(moveSequence.ToString(), depth, moveText);

                    b.DoMove(gameMove);
                    analyzedMoves.Add(analyzedMove);
                    moveSequence.Append(" " + moveText);
                    beforeFen = b.DumpFen();
                }

                // comment the game
                CalculateDeltas(analyzedMoves);
                Comment(analyzedMoves);
                return analyzedMoves.ToArray();
            }
        }

        private static void CalculateDeltas(List<AnalyzedMove> analyzedMoves)
        {
            for (int i = 0; i < analyzedMoves.Count(); i++)
            {
                if (analyzedMoves[i].ActualMove != null)
                    analyzedMoves[i].ActualMove.DeltaToBest = analyzedMoves[i].ActualMove.Value -
                                                              analyzedMoves[i].BestMove.Value;
            }
        }

        private static void Comment(List<AnalyzedMove> analyzedMoves)
        {
            for (int i = 1; i < analyzedMoves.Count(); i++)
            {
                var cur = analyzedMoves[i].ActualMove ?? analyzedMoves[i].BestMove;
                var prev = analyzedMoves[i-1].ActualMove ?? analyzedMoves[i-1].BestMove;
                var dPrev = prev.DeltaToBest;
                var dCur = cur.DeltaToBest;
                bool blunderPrev = dPrev < -100;
                bool blunderCur = dCur < -100;
                if (blunderPrev && blunderCur && cur.Value < 200)
                {
                    // missed
                    analyzedMoves[i].Category = 1;
                }
                if (!blunderPrev && blunderCur && cur.Value < 200)
                {
                    // blunder
                    analyzedMoves[i].Category = 2;
                }
                if (blunderPrev && !blunderCur && cur.Value > -200)
                {
                    // good
                    analyzedMoves[i].Category = 3;
                }
                if (!blunderPrev && !blunderCur)
                {
                    // nothing
                    analyzedMoves[i].Category = 4;
                }
            }
        }
        


        public static int MaterialScore(Board b)
        {

            if (b.IsDraw)
                return 0;
            else if (b.IsMate)
            {
                return 30000 * (b.WhitesTurn ? -1 : 1);
            }
            else
            {
                int[] pieceCount = b.PieceCount();
                int[] scores = new int[] { 0, 100, 500, 300, 300, 900, 0, -100, -500, -300, -300, -900, 0, 0, 0, 0, 0 };
                int totalScore = 0;
                for (int i = 0; i < pieceCount.Length; i++)
                    totalScore += pieceCount[i] * scores[i];
                return totalScore;
            }
        }
    }
}
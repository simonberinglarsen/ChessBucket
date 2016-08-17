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
        public static List<AnalyzedMove> Game(List<string> moveList)
        {
            using (var proxy = new SFProxy())
            {
                Board b = new Board();
                b.StartPosition();
                List<AnalyzedMove> analyzedMoves = new List<AnalyzedMove>();
                var doubleTest = "";
                foreach (var moveText in moveList)
                {
                    System.Diagnostics.Debug.WriteLine(moveText);
                    Move gameMove = b.MoveFromText(moveText);
                    AnalyzedMove analyzedMove = new AnalyzedMove();
                    analyzedMove.IsWhite = b.WhitesTurn;
                    analyzedMove.BeforeFen = b.DumpFen();

                    string fen = proxy.FenAfterMoves(doubleTest).Substring(5);
                    if (fen != analyzedMove.BeforeFen)
                    {
                        throw new Exception("last move failed! - >" + doubleTest);
                    }

                    analyzedMove.AllMoves = AllMovesFromPosition(analyzedMove.BeforeFen, b, proxy);
                    analyzedMove.ActualMoveIndex = Array.IndexOf(analyzedMove.AllMoves, analyzedMove.AllMoves.Single(m => m.MoveLan == moveText));
                    b.DoMove(gameMove);
                    analyzedMoves.Add(analyzedMove);
                    doubleTest += " " + moveText;
                }

                // comment the game
                Comment(analyzedMoves);

                return analyzedMoves;
            }
        }

        private static void Comment(List<AnalyzedMove> analyzedMoves)
        {
            for (int i = 1; i < analyzedMoves.Count(); i++)
            {
                var cur = analyzedMoves[i].AllMoves[analyzedMoves[i].ActualMoveIndex];
                var prev = analyzedMoves[i - 1].AllMoves[analyzedMoves[i - 1].ActualMoveIndex];
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


        private static EvaluatedMove[] AllMovesFromPosition(string fen, Board b, SFProxy proxy)
        {
            // get SF results
            string[] pvs = proxy.Go(fen, 10);
            EvaluatedMove[] all = new EvaluatedMove[pvs.Length];
            for (int i = 0; i < pvs.Length; i++)
            {
                var pv = pvs[i];
                var genMoves = b.GenerateMoves();
                b.PopulateSan(genMoves);
                int firstMoveIndex = pv.IndexOf(' ');
                int value = int.Parse(pv.Substring(0, firstMoveIndex));
                string moveLan = pv.Substring(firstMoveIndex + 1, 4);
                var z = genMoves.Single(m => m.Text == moveLan);
                b.DoMove(z);
                all[i] = new EvaluatedMove()
                {
                    Value = value,
                    MoveLan = moveLan,
                    DeltaToBest = i > 0 ? value - all[0].Value : 0,
                    MoveSan = z.San,
                    Fen = b.DumpFen(),
                };
                b.Setup(fen);
            }
            return all;
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
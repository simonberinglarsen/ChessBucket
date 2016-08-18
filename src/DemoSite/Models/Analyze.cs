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
            int depth = 5;
            string beforeFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            using (var proxy = new SFProxy())
            {
                Board b = new Board();
                b.StartPosition();
                List<AnalyzedMove> analyzedMoves = new List<AnalyzedMove>();
                StringBuilder moveSequence = new StringBuilder();
                foreach (var moveText in moveList)
                {
                    System.Diagnostics.Debug.WriteLine(moveText);
                    Move gameMove = b.MoveFromText(moveText);
                    AnalyzedMove analyzedMove = new AnalyzedMove();
                    analyzedMove.IsWhite = b.WhitesTurn;
                    // sanity check ... to be removed
                    string fen = proxy.FenAfterMoves(moveSequence.ToString()).Substring(5);
                    if (fen != beforeFen)
                    {
                        throw new Exception("last move failed! - >" + moveSequence.ToString());
                    }
                    // array of actual- and bestmove
                    string bestMove = proxy.Go(moveSequence.ToString(), depth);
                    string actualMove = proxy.Go(moveSequence.ToString() + " " + moveText, depth);
                    var bestMoveEval = EvaluatedMove(b, bestMove);
                    var actualMoveEval = EvaluatedMove(b, actualMove, moveText);
                    analyzedMove.ActualMoveIndex = 0;
                    if (bestMoveEval.MoveLan == actualMoveEval.MoveLan || bestMoveEval.Value <= actualMoveEval.Value)
                        analyzedMove.AllMoves = new Models.EvaluatedMove[] {actualMoveEval};
                    else
                    {
                        analyzedMove.ActualMoveIndex = 1;
                        analyzedMove.AllMoves = new Models.EvaluatedMove[] { bestMoveEval, actualMoveEval };
                    }

                    b.DoMove(gameMove);
                    analyzedMoves.Add(analyzedMove);
                    moveSequence.Append(" " + moveText);
                    beforeFen = analyzedMove.AllMoves[analyzedMove.ActualMoveIndex].Fen;
                }

                // comment the game
                CalculateDeltas(analyzedMoves);
                Comment(analyzedMoves);
                return analyzedMoves;
            }
        }

        private static void CalculateDeltas(List<AnalyzedMove> analyzedMoves)
        {
            for (int i = 0; i < analyzedMoves.Count(); i++)
            {
                int bestValue = analyzedMoves[i].AllMoves[0].Value;
                foreach (var m in analyzedMoves[i].AllMoves)
                {
                    m.DeltaToBest = m.Value - bestValue;
                }
                
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

        private static EvaluatedMove EvaluatedMove(Board b, string evaluation, string hackMove = null)
        {
            string fen = b.DumpFen();
            var genMoves = b.GenerateMoves();
            b.PopulateSan(genMoves);
            int firstMoveIndex = evaluation.IndexOf(' ');
            string valueText = evaluation.Substring(0, firstMoveIndex);
            int value = hackMove != null ? -int.Parse(valueText) : int.Parse(valueText);
            string moveLan = hackMove ?? evaluation.Substring(firstMoveIndex + 1, 4);
            var z = genMoves.Single(m => m.Text == moveLan);
            b.DoMove(z);
            EvaluatedMove all = new EvaluatedMove()
            {
                Value = value,
                MoveLan = moveLan,
                DeltaToBest = 0,
                MoveSan = z.San,
                Fen = b.DumpFen(),
            };
            b.Setup(fen);
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
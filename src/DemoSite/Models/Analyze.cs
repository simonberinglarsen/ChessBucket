using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DemoSite.Models
{
    public class Analyze
    {
        public static List<EvaluatedMoveWithAlternatives> Game(List<string> moveList)
        {
            Board b = new Board();
            b.StartPosition();
            List<EvaluatedMoveWithAlternatives> evaluation = new List<EvaluatedMoveWithAlternatives>();
            foreach (var moveText in moveList)
            {
                Move move = Move.FromText(moveText);
                EvaluatedMoveWithAlternatives eval = new EvaluatedMoveWithAlternatives();
                eval.BeforeFen = b.DumpFen();
                eval.Move = move.Text;
                Move[] availableMoves = b.GenerateMoves();
                eval.AvailableMoves = new List<EvaluatedMove>();
                foreach (var availableMove in availableMoves)
                {
                    b.DoMove(availableMove);
                    eval.AvailableMoves.Add(new EvaluatedMove()
                    {
                        Move = availableMove.Text,
                        AfterCentiPawns = Board(b),
                        AfterFen = b.DumpFen()
                    });
                    b.Setup(eval.BeforeFen);
                }
                if (b.WhitesTurn)
                    eval.AvailableMoves = eval.AvailableMoves.OrderByDescending(x => x.AfterCentiPawns).ToList();
                else
                    eval.AvailableMoves = eval.AvailableMoves.OrderBy(x => x.AfterCentiPawns).ToList();
                b.DoMove(move);
                eval.AfterFen = b.DumpFen();
                eval.AfterCentiPawns = Board(b);
                evaluation.Add(eval);
            }
            return evaluation;
        }

        public static int Board(Board b)
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
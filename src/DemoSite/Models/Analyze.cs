using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Tools.Internal;

namespace DemoSite.Models
{
    public class Analyze
    {
        public int _depth;
        public IJob _job;
        private const string Engine = "Stockfish";
        public Analyze(int depth, IJob job)
        {
            _depth = depth;
            _job = job;
        }

        public string Info => $"Engine: {Engine}, Depth:{_depth}";

        public AnalyzedMove[] Game(string[] moveList)
        {
            System.Diagnostics.Debug.WriteLine("Analyze.Game");
            string beforeFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            using (var proxy = new SFProxy())
            {
                Board b = new Board();
                b.StartPosition();
                List<AnalyzedMove> analyzedMoves = new List<AnalyzedMove>();
                StringBuilder moveSequence = new StringBuilder();
                for (int i = 0; i < moveList.Length; i++)
                {
                    string logLine = $"Analyze.Game {((i+1)/(double) moveList.Length).ToString("0.##%")}";
                    System.Diagnostics.Debug.WriteLine(logLine);
                    _job.Ping(logLine);
                    if (_job.QState == QState.Cancel)
                        throw new CancelJobException("job was cancelled (by changing qstate)");
                    string moveText = moveList[i];
                    Move gameMove = Move.FromLan(moveText);
                    AnalyzedMove analyzedMove = new AnalyzedMove();
                    analyzedMove.IsWhite = b.WhitesTurn;
                    // sanity check ... to be removed
                    string fen = proxy.FenAfterMoves(moveSequence.ToString());
                    if (fen != beforeFen)
                    {
                        throw new Exception("last move failed! - >" + moveSequence.ToString());
                    }
                    // array of actual- and bestmove
                    analyzedMove.BestMove = proxy.Go(moveSequence, _depth);
                    if (analyzedMove.BestMove.MoveLan != moveText)
                        analyzedMove.ActualMove = proxy.Go(moveSequence, _depth, moveText);

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
                var prev = analyzedMoves[i - 1].ActualMove ?? analyzedMoves[i - 1].BestMove;
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

    public class CancelJobException : Exception
    {
        public CancelJobException(string message) : base(message)
        {
            
        }
    }
}
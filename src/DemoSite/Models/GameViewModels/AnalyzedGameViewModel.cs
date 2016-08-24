using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models.GameViewModels
{
    public class AnalyzedGameViewModel
    {
        public List<AnalyzedMoveViewModel> AnalyzedMoves { get; set; }
    }
    public class AnalyzedMoveViewModel
    {
        public bool IsWhite { get; set; }
        public string Description { get; set; }
        public EvaluatedMoveViewModel BestMove { get; set; }
        public EvaluatedMoveViewModel ActualMove { get; set; }
        public int Category { get; set; }

        internal static AnalyzedMoveViewModel MapFrom(AnalyzedMove from, Board b)
        {
            AnalyzedMoveViewModel to = new AnalyzedMoveViewModel()
            {
                ActualMove = EvaluatedMoveViewModel.MapFrom(from.ActualMove, b),
                BestMove = EvaluatedMoveViewModel.MapFrom(from.BestMove, b),
                Category = from.Category,
                IsWhite = from.IsWhite
            };
            return to;
        }
    }

    public class EvaluatedMoveViewModel
    {
        public string Fen { get; set; }
        public string MoveSan { get; set; }
        public string MoveLan { get; set; }
        public int Value { get; set; }
        public int DeltaToBest { get; set; }
        public EvaluatedMoveViewModel[] PrincipalVariation { get; set; }

        internal static EvaluatedMoveViewModel MapFrom(EvaluatedMove from, Board b)
        {
            if (from == null) return null;
            string beforeFen = b.DumpFen();
            EvaluatedMoveViewModel to = new EvaluatedMoveViewModel() {
                DeltaToBest = from.DeltaToBest,
                MoveLan = from.MoveLan,
                MoveSan = from.MoveSan,
                Value = from.Value
            };
            if(from.PrincipalVariation != null)
            {
                List<EvaluatedMoveViewModel> pv = new List<EvaluatedMoveViewModel>();
                foreach(var pvMove in from.PrincipalVariation)
                {
                    pv.Add(EvaluatedMoveViewModel.MapFrom(pvMove, b));
                    b.DoMove(Move.FromLan(pvMove.MoveLan));
                }
                to.PrincipalVariation = pv.ToArray();
                b.Setup(beforeFen);
            }
            b.DoMove(Move.FromLan(from.MoveLan));
            to.Fen = b.DumpFen();
            b.Setup(beforeFen);
            return to;
        }
    }
}

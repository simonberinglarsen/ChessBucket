using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models.BoardViewModels
{
    public class AnalyzedGameViewModel
    {
        public List<ColoredEvaluatedMove> EvaluatedMoves { get; set; }

    }

    public class ColoredEvaluatedMove
    {
        public int MoveNumber { get; set; }
        public EvaluatedMoveWithAlternatives White { get; set; }
        public EvaluatedMoveWithAlternatives Black { get; set; }
    }
}

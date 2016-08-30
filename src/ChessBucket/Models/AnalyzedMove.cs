using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessBucket.Models
{
    public class AnalyzedMove
    {
        public bool IsWhite { get; set; }
        public string Description { get; set; }
        public EvaluatedMove BestMove { get; set; }
        public EvaluatedMove ActualMove { get; set; }
        public int Category { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DemoSite.Models
{
    public class EvaluatedMoveWithAlternatives
    {
        public string BeforeFen { get; set; }
        public string AfterFen { get; set; }
        public int AfterCentiPawns { get; set; }
        public string Move { get; set; }
        public List<EvaluatedMove> AvailableMoves { get; set; }
    }

    public class EvaluatedMove
    {
        public string AfterFen { get; set; }
        public int AfterCentiPawns { get; set; }
        public string Move { get; set; }
    }
}
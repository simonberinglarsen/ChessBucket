using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChessBucket.Models
{
   

    public class EvaluatedMove
    {
        public string MoveSan { get; set; }
        public string MoveLan { get; set; }
        public int Value { get; set; }
        public int DeltaToBest { get; set; }
        public EvaluatedMove[] PrincipalVariation { get; set; }

    }

}
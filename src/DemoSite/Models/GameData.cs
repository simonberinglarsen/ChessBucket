using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models
{
    public class GameData
    {
        public int Id { get; set; }
        public string[] MovesLan { get; set; }
        public string[] MovesSan { get; set; }
        public bool Analyzed { get; set; }
        public AnalyzedMove[] AnalyzedMoves { get; set; }
        public string Tag { get; set; }
    }
}

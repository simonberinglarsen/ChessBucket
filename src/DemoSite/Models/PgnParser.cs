using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DemoSite.Models;

namespace ConsoleApp1
{
    public class PgnParser
    {
        private string _pgnHeader;
        private string _pgnMoves;
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public string[] MovesLan { get; set; }
        public string[] MovesSan { get; set; }
        public void LoadPgn(string pgn)
        {
            int gameIndex = pgn.IndexOf("1.");
            _pgnHeader = pgn.Substring(0, gameIndex);
            _pgnMoves = pgn.Substring(gameIndex);
            ParseHeader();
            ParseMoves();
        }

        private void ParseHeader()
        {
            string[] elements = _pgnHeader.Split(new char[] { ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var element in elements)
            {
                var trimmed = element.Substring(1).Trim('\r', '\n');
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;
                var quoteIndex = trimmed.IndexOf('\"');
                var key = trimmed.Substring(0, quoteIndex).Trim();
                var value = trimmed.Substring(quoteIndex).Trim('\"');
                Headers.Add(key, value);
            }
        }

        private void ParseMoves()
        {
            Board b = new Board();
            b.StartPosition();
            string[] elements = _pgnMoves.ToLower().Replace('\r', ' ').Replace('\n', ' ').Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> lan = new List<string>();
            List<string> san = new List<string>();
            for (int i = 0; i < elements.Length; i++)
            {

                if (elements[i] == "1-0" ||
                    elements[i] == "0-1" ||
                    elements[i] == "1/2-1/2" ||
                    string.IsNullOrWhiteSpace(elements[i]))
                    break;
                if (i % 3 == 0)
                    continue;
                Debug.WriteLine("parsing move: "+elements[i]);
                var moves = b.GenerateMoves();
                b.PopulateSan(moves);
                var r = moves.Single(x => x.San.ToLower() == elements[i].Replace("+", ""));
                lan.Add(r.Text);
                san.Add(r.San);
                
                b.DoMove(r);
            }
            MovesLan = lan.ToArray();
            MovesSan = san.ToArray();

        }
    }
}

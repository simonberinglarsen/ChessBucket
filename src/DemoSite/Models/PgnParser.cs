using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DemoSite.Models;
using System.Text;

namespace DemoSite.Models
{
    public class PgnParser
    {
        public PgnGame[] PgnGames;

        public void LoadPgn(string pgn)
        {
            Parse(pgn);
        }

        private enum State
        {
            NewGame,
            FindHeaderOrGame,
            ReadHeader,
            ReadGame
        }

        private void Parse(string pgn)
        {
            Debug.WriteLine("PgnParser.Parse");
            int i = 0;
            string[] e;
            State state = State.NewGame;
            List<PgnGame> all = new List<PgnGame>();
            StringBuilder buffer = new StringBuilder();
            while (i < pgn.Length)
            {
                buffer.Append(pgn[i]);
                switch (state)
                {
                    case State.NewGame:
                        all.Add(new PgnGame());
                        state = State.FindHeaderOrGame;
                        continue;
                    case State.FindHeaderOrGame:
                        if (" \n\r".IndexOf(pgn[i]) > 0)
                        {
                            i++;
                            continue;
                        }
                        else if (pgn[i] == '[')
                        {
                            buffer.Clear();
                            state = State.ReadHeader;
                            i++;
                            continue;
                        }
                        else if (pgn[i] == '1')
                        {
                            state = State.ReadGame;
                        }
                        else
                            throw new Exception("unknown character while searching for header or game!");
                        break;
                    case State.ReadHeader:
                        if (pgn[i] == ']')
                        {
                            string header = buffer.ToString();
                            e = header.Split(new char[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                            string tag = e[0].Trim();
                            string value = e[1].Trim();
                            var last = all.Last();
                            last.Headers.Add(tag.ToUpper(), value);
                            buffer.Clear();
                            i++;
                            state = State.FindHeaderOrGame;
                            continue;
                        }
                        break;
                    case State.ReadGame:
                        if (pgn[i] == '[' || i == pgn.Length - 1)
                        {
                            string moves = buffer.ToString();
                            var last = all.Last();
                            ParseMoves(last, moves);
                            buffer.Clear();
                            state = State.NewGame;
                            if (i == pgn.Length - 1) break;
                            continue;
                        }
                        break;
                    default:
                        throw new Exception("unknown state in pgn parser!");
                }
                i++;
            }
            PgnGames = all.ToArray();

        }

       

        private void ParseMoves(PgnGame pgnGame, string pgnMoves)
        {
            Debug.WriteLine("PgnParser.ParseMoves");
            Board b = new Board();
            b.StartPosition();
            string[] elements = pgnMoves.Replace('\r', ' ').Replace('\n', ' ').Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
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
                var moves = b.GenerateMoves();
                b.PopulateSan(moves);
                try
                {
                    var r = moves.Single(x => x.San == elements[i].Replace("+", "").Replace("#", ""));
                    lan.Add(r.Lan);
                    san.Add(r.San);
                    b.DoMove(r);
                }
                catch(Exception ex)
                {
                    throw new Exception("move not found");
                }

                
            }
            pgnGame.MovesLan = lan.ToArray();
            pgnGame.MovesSan = san.ToArray();
        }
    }

    public class PgnGame
    {
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public string[] MovesLan { get; set; }
        public string[] MovesSan { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class SFProxy : IDisposable
    {
        private const string SFPath = @"C:\Users\bc0618\Desktop\simon\chess\stockfish-7-win\Windows\stockfish 7 x64.exe";
        private readonly Process _proc;
        public SFProxy()
        {
            string path = @"C:\Users\bc0618\Desktop\simon\chess\stockfish-7-win\Windows\stockfish 7 x64.exe";
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };
            _proc.Start();
        }

        public string[] Go(string fen, int depth)
        {
            _proc.StandardInput.WriteLine("setoption name multipv value 500");
            _proc.StandardInput.WriteLine("position fen " + fen);
            _proc.StandardInput.WriteLine("go depth " + depth);
            string[] multipv = new string[500];
            int maxindex = 0;
            while (!_proc.StandardOutput.EndOfStream)
            {
                string line = _proc.StandardOutput.ReadLine();
                if (line.IndexOf("info depth " + depth) == 0)
                {
                    int index1 = line.IndexOf("multipv");
                    if (index1 == -1)
                        continue;
                    index1 += 7;
                    int index2 = line.IndexOf("score");
                    string multipvIndexText = line.Substring(index1, index2 - index1).Trim();
                    int multipvIndex = int.Parse(multipvIndexText) - 1;
                    maxindex = Math.Max(maxindex, multipvIndex);

                    //construct line
                    int q1 = line.IndexOf(" score ")+7;
                    int q2 = line.IndexOf(" pv ")+4;
                    string[] score = line.Substring(q1).Split(new char[] {' '});
                    if (score[0] == "mate")
                        score[1] = "-30000";
                    var constructedLine = score[1]+" "+line.Substring(q2);
                    
                    Debug.WriteLine("constructedLine = "+ constructedLine);

                    multipv[multipvIndex] = constructedLine;
                }
                else if (line.IndexOf("bestmove") == 0)
                {
                    break;
                }
            }
            Array.Resize(ref multipv, maxindex + 1);
            return multipv;
        }

        public string FenAfterMoves(string moves)
        {
            _proc.StandardInput.WriteLine("setoption name multipv value 500");
            _proc.StandardInput.WriteLine("position startpos moves " + moves);
            _proc.StandardInput.WriteLine("d");
            while (!_proc.StandardOutput.EndOfStream)
            {
                string line = _proc.StandardOutput.ReadLine();
                if (line.IndexOf("Fen:") == 0)
                {
                    return line;
                }
            }
            return "";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _proc.StandardInput.WriteLine("quit");
            if (!_proc.WaitForExit(5000))
            {
                _proc.Kill();
            }
            _disposed = true;
        }
    }
}

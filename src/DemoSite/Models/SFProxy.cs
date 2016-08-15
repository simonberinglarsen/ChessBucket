using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class SFProxy
    {
        public void Go()
        {
            string path = @"C:\Users\bc0618\Desktop\simon\chess\stockfish-7-win\Windows\stockfish 7 x64.exe";
            var proc = new Process
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
            proc.Start();
            List<string> infos = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.IndexOf("Stockfish 7") == 0)
                {
                    // setup
                    proc.StandardInput.WriteLine("setoption name multipv value 500");
                    proc.StandardInput.WriteLine("position startpos");
                    proc.StandardInput.WriteLine("go depth 5");
                }
                else if (line.IndexOf("info depth 5") == 0)
                {
                    infos.Add(line);
                }
                else if (line.IndexOf("bestmove") == 0)
                {
                    infos.Clear();
                }


            }
        }
    }
}

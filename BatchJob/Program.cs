using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BatchJob
{
    class Program
    {
        static void Main(string[] args)

        {
            string url = @"http://localhost:5000/Admin/TriggerQProcessor";
            WebClient c = new WebClient();
            while (true)
            {
                try
                {
                    LogInfo(DateTime.Now + ": triggering analysis...");
                    c.DownloadData(url);
                    LogSuccess("done");
                }
                catch (Exception ex)
                {
                    LogError("failed: " + ex.Message);
                }
                LogNewLine();
                System.Threading.Thread.Sleep(10000);
            }
        }

        private static void LogNewLine()
        {
            Console.WriteLine();
        }

        private static void LogInfo(string text)
        {
            Log(text, Console.ForegroundColor);
        }
        private static void LogError(string text)
        {
            Log(text, ConsoleColor.Red);
        }
        private static void LogSuccess(string text)
        {
            Log(text, ConsoleColor.Green);
        }

        private static void Log(string text, ConsoleColor color)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = temp;
        }


    }
}


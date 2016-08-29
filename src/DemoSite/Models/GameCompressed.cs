using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace DemoSite.Models
{
    public class GameCompressed
    {
        public int Id { get; set; }
        public AnalysisState AnalysisState { get; set; }
        public string AnalysisInfo { get; set; }
        //The name of the tournament or match event.
        public string Event { get; set; }
        //The location of the event. This is in "City, Region COUNTRY" format, where COUNTRY is the three-letter International Olympic Committee code for the country. An example is "New York City, NY USA".
        public string Site { get; set; }
        //The starting date of the game, in YYYY.MM.DD form. "??" are used for unknown values.
        public string Date { get; set; }
        //The playing round ordinal of the game within the event.
        public string Round { get; set; }
        //The player of the white pieces, in "last name, first name" format.
        public string White { get; set; }
        //The player of the black pieces, same format as White.
        public string Black { get; set; }
        //The result of the game.This can only have four possible values: "1-0" (White won), "0-1" (Black won), "1/2-1/2" (Draw), or "*" (other, e.g., the game is ongoing).
        public string Result { get; set; }
        public byte[] CompressedData { get; set; }
        public string WhiteElo { get; set; }
        public string BlackElo { get; set; }
        public string EventCountry { get; set; }
    }
}

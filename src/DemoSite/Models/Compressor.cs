using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DemoSite.Models
{
    public static class Compressor
    {
        private class CompressedData
        {
            public string[] MovesLan { get; set; }
            public string[] MovesSan { get; set; }
            public AnalyzedMove[] AnalyzedMoves { get; set; }
        }
        public static GameCompressed Compress(Game input)
        {
            var objectToCompress = new CompressedData()
            {
                MovesLan = input.MovesLan,
                MovesSan = input.MovesSan,
                AnalyzedMoves = input.AnalyzedMoves,
            };
            string json = JsonConvert.SerializeObject(objectToCompress);
            byte[] compressedData;
            using (var binaryStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(binaryStream, CompressionMode.Compress))
                {
                    using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        jsonStream.CopyTo(zipStream);
                    }
                }
                compressedData = binaryStream.ToArray();
            }
            GameCompressed compressed = new GameCompressed()
            {
                Id = input.Id,
                AnalysisState = input.AnalysisState,
                AnalysisInfo = input.AnalysisInfo,
                Event = input.Event,
                Site = input.Site,
                Date = input.Date,
                Round = input.Round,
                White = input.White,
                Black = input.Black,
                WhiteElo = input.WhiteElo,
                BlackElo = input.BlackElo,
                EventCountry = input.EventCountry,
                Result = input.Result,
                CompressedData = compressedData
            };

            return compressed;
        }

        public static Game Decompress(GameCompressed input)
        {

            string json;
            using (var binaryStream = new MemoryStream(input.CompressedData))
            {
                using (var unzipStream = new GZipStream(binaryStream, CompressionMode.Decompress))
                {
                    using (var jsonStream = new MemoryStream())
                    {
                        unzipStream.CopyTo(jsonStream);
                        json = Encoding.UTF8.GetString(jsonStream.ToArray());
                    }
                }
            }
            CompressedData decompressedObject = JsonConvert.DeserializeObject<CompressedData>(json);
            Game decompressed = new Game()
            {
                Id = input.Id,
                AnalysisState = input.AnalysisState,
                AnalysisInfo = input.AnalysisInfo,
                Event = input.Event,
                Site = input.Site,
                Date = input.Date,
                Round = input.Round,
                White = input.White,
                Black = input.Black,
                WhiteElo = input.WhiteElo,
                BlackElo = input.BlackElo,
                EventCountry = input.EventCountry,
                Result = input.Result,
                MovesSan = decompressedObject.MovesSan,
                MovesLan = decompressedObject.MovesLan,
                AnalyzedMoves = decompressedObject.AnalyzedMoves
            };
            return decompressed;
        }
    }
}

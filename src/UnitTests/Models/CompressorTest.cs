using DemoSite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Models
{
    public class CompressorTest
    {
        [Fact]
        public void DecompressedGameWithAllPropertiesPopulated_CompressAndDecompress_SameGameIsObtained()
        {
            
            GameDecompressedBuilder builder = new GameDecompressedBuilder();
            Game decompressed = builder.RandomData().Build();
            var compressed = Compressor.Compress(decompressed);
            var decompressed2 = Compressor.Decompress(compressed);
            Assert.True(decompressed.Id == decompressed2.Id);
            Assert.True(decompressed.AnalysisState == decompressed2.AnalysisState);
            Assert.True(decompressed.AnalysisInfo == decompressed2.AnalysisInfo);
            Assert.True(decompressed.Event == decompressed2.Event);
            Assert.True(decompressed.Site == decompressed2.Site);
            Assert.True(decompressed.Date == decompressed2.Date);
            Assert.True(decompressed.Round == decompressed2.Round);
            Assert.True(decompressed.White == decompressed2.White);
            Assert.True(decompressed.Black == decompressed2.Black);
            Assert.True(decompressed.WhiteElo == decompressed2.WhiteElo);
            Assert.True(decompressed.BlackElo == decompressed2.BlackElo);
            Assert.True(decompressed.EventCountry  == decompressed2.EventCountry);
            Assert.True(decompressed.Result == decompressed2.Result);
            AssertHelper.True(decompressed.AnalyzedMoves, decompressed2.AnalyzedMoves);
            AssertHelper.True(decompressed.MovesLan, decompressed2.MovesLan);
            AssertHelper.True(decompressed.MovesSan, decompressed2.MovesSan);
        }
    }

    public static class AssertHelper
    {
        public static void True(AnalyzedMove[] a, AnalyzedMove[] b)
        {
            if (a == null && b == null) return;
            Assert.True(a != null && b != null);
            Assert.True(a.Length == b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                AssertHelper.True(a[i], b[i]);
            }
        }

        private static void True(AnalyzedMove a, AnalyzedMove b)
        {
            Assert.True(a.IsWhite == b.IsWhite);
            Assert.True(a.Category == b.Category);
            Assert.True(a.Description == b.Description);
            AssertHelper.True(a.ActualMove, b.ActualMove);
            AssertHelper.True(a.BestMove, b.BestMove);
        }

        private static void True(EvaluatedMove a, EvaluatedMove b)
        {
            Assert.True(a.Value == b.Value);
            Assert.True(a.DeltaToBest == b.DeltaToBest);
            Assert.True(a.MoveLan == b.MoveLan);
            Assert.True(a.MoveSan == b.MoveSan);
            AssertHelper.True(a.PrincipalVariation, a.PrincipalVariation);
        }

        private static void True(EvaluatedMove[] a, EvaluatedMove[] b)
        {
            if (a == null && b == null) return;
            Assert.True(a != null && b != null);
            Assert.True(a.Length == b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                AssertHelper.True(a[i], b[i]);
            }
        }

        internal static void True(string[] a, string[] b)
        {
            if (a == null && b == null) return;
            Assert.True(a != null && b != null);
            Assert.True(a.Length == b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.True(a[i].Equals(b[i]));
            }
        }
    }

    public class GameDecompressedBuilder
    {
        Game x = new Game();   
        public GameDecompressedBuilder RandomData()
        {
            x.AnalysisState = AnalysisState.Pending;
            x.Date = RandomStuff.String();
            x.Event = RandomStuff.String();
            x.Id = RandomStuff.Int();
            x.MovesLan = new string[] {
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
            };
            x.MovesSan = new string[]  {
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
                RandomStuff.String(),
            };
            x.Result = RandomStuff.String();
            x.Round = RandomStuff.String();
            x.Site = RandomStuff.String();
            x.AnalysisInfo = RandomStuff.String();
            x.Black = RandomStuff.String();
            x.White = RandomStuff.String();
            x.BlackElo = (1200 + 9*RandomStuff.Int()).ToString();
            x.WhiteElo = (1200 + 9 * RandomStuff.Int()).ToString();
            x.EventCountry = RandomStuff.String();
            x.AnalyzedMoves = new[]
            {
               new AnalyzedMoveBuilder().Random().Build(),
               new AnalyzedMoveBuilder().Random().Build(),
               new AnalyzedMoveBuilder().Random().Build(),
               new AnalyzedMoveBuilder().Random().Build(),
               new AnalyzedMoveBuilder().Random().Build()
            };
            return this;
        }

        public Game Build()
        {
            return x;
        }
    }

    public class EvaluatedMoveBuilder
    {
        private EvaluatedMove e = new EvaluatedMove();
        public EvaluatedMoveBuilder Random()
        {
            e.Value = RandomStuff.Int();
            e.MoveLan = RandomStuff.String(4);
            e.DeltaToBest = RandomStuff.Int();
            e.MoveSan = RandomStuff.String(4);
            return this;
        }
        public EvaluatedMove Build()
        {
            return e;
        }

        public EvaluatedMoveBuilder WithPrincipalVariation()
        {
            e.PrincipalVariation = new EvaluatedMove[]
            {
                new EvaluatedMoveBuilder().Random().Build(),
                new EvaluatedMoveBuilder().Random().Build(),
                new EvaluatedMoveBuilder().Random().Build(),
            };
            return this;
        }
    }

    class RandomStuff
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        public static bool Bool()
        {
            return rnd.Next(2) == 0;
        }

        public static int Int()
        {
            return rnd.Next(100);
        }

        public static string String()
        {
            return String(rnd.Next(5) + 5);
        }

        internal static string String(int length)
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append((char)('a' + rnd.Next('z' - 'a')));
            }
            return sb.ToString();
        }
    }
    internal class AnalyzedMoveBuilder : AnalyzedMove
    {
        private AnalyzedMove x = new AnalyzedMove();
        public AnalyzedMoveBuilder Random()
        {
            x.ActualMove = new EvaluatedMoveBuilder().Random().WithPrincipalVariation().Build();
            x.BestMove = new EvaluatedMoveBuilder().Random().WithPrincipalVariation().Build();
            x.IsWhite = RandomStuff.Bool();
            x.Category = RandomStuff.Int();
            x.Description = RandomStuff.String();
            return this;
        }

        public AnalyzedMove Build()
        {
            return x;
        }
    }
}

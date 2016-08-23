using DemoSite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Models
{
    public class BoardTest
    {
        const string jsonExpectedPath = "Testdata\\sinqcup16_expected_board.json";
        [Fact]
        public void TournamentSet_PlayThroughAndGenerateAlternativeMoves_NoRulesBroken()
        {
            PgnParser parser = new PgnParser();
            parser.LoadPgn(System.IO.File.ReadAllText("Testdata\\sinqcup16.pgn"));
            List<TestGameResults> allGames = new List<TestGameResults>();
            TestGameResults[] expectedResults = JsonConvert.DeserializeObject<TestGameResults[]>(File.ReadAllText(jsonExpectedPath));
            for (int i=0; i< parser.PgnGames.Length; i++)
            {
                var game = parser.PgnGames[i];
                //var  x = Analyze.Game(game.MovesLan);
                Board b = new Board();
                TestGameResults testGame = new TestGameResults();
                allGames.Add(testGame);

                b.StartPosition();
                for(int j=0; j< game.MovesLan.Length; j++)
                {
                    var move = game.MovesLan[j];
                    var testMove = new TestMoveResult();
                    testGame.TestMoves.Add(testMove);

                    var genmoves = b.GenerateMoves();
                    b.PopulateSan(genmoves);
                    string beforeFen = b.DumpFen();
                    for(int k=0; k < genmoves.Length; k++)
                    {
                        
                        var genmove = genmoves[k];
                        var genTestMove = new TestMoveResult();
                        testMove.Generated.Add(genTestMove);
                        genTestMove.Lan = genmove.Lan;
                        genTestMove.San = genmove.San;
                        b.DoMove(genmove);
                        genTestMove.Fen = b.DumpFen();
                        b.Setup(beforeFen);

                        // verify
                        Assert.True(genTestMove.Lan == expectedResults[i].TestMoves[j].Generated[k].Lan);
                        Assert.True(genTestMove.San == expectedResults[i].TestMoves[j].Generated[k].San);
                        Assert.True(genTestMove.Fen == expectedResults[i].TestMoves[j].Generated[k].Fen);
                    }
                    testMove.Lan = move;
                    testMove.San = genmoves.Single(x => x.Lan == move).San;
                    b.DoMove(Move.FromLan(move));
                    testMove.Fen = b.DumpFen();

                    // verify
                    Assert.True(testMove.Lan == expectedResults[i].TestMoves[j].Lan);
                    Assert.True(testMove.San == expectedResults[i].TestMoves[j].San);
                    Assert.True(testMove.Fen == expectedResults[i].TestMoves[j].Fen);
                }
            }
            // comment in below lines to recreated test data..
            //var expectedData = JsonConvert.SerializeObject(allGames);
            //File.WriteAllText(jsonExpectedPath, expectedData);
        }
    }

    public class TestGameResults
    {
        public List<TestMoveResult> TestMoves = new List<TestMoveResult>();
    }
    public class TestMoveResult
    {
        public List<TestMoveResult> Generated = new List<TestMoveResult>();

        public string Fen { get; set; }
        public string Lan { get; set; }
        public string San { get; set; }
    }
}

using DemoSite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Models
{
    public class BoardTest
    {
        const string jsonExpectedPath = "Testdata\\sinqcup16_expected_board.json";

        private readonly ITestOutputHelper output;

        public BoardTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void VariationWithNge2InstreadofNe2_ConvertingToLan_AllSanMovesFound()
        {
            string pgn = @"
[Event ""Sydkysten Efteraar Mester 1 ""]
[Site ""Greve""]
[Date ""1999.10.23""]
[Round ""3""]
[White ""Kristensen, Brian""]
[Black ""Askgaard, Jens""]
[Result ""1/2-1/2""]
[ECO ""C01""]
[WhiteElo ""2075""]
[BlackElo ""2088""]
[PlyCount ""43""]
[EventDate ""1999.??.??""]
[EventType ""tourn""]
[EventRounds ""5""]
[EventCountry ""DEN""]

1. e4 e6 2. d4 d5 3. exd5 exd5 4. c4 Nf6 5. Nc3 Bb4 6. Bd3 O-O 7. Nge2 dxc4 8.
Bxc4 Nbd7 9. O-O Nb6 10. Bb3 c6 11. Bg5 Qd6 12. Bxf6 Qxf6 13. Ne4 Qh4 14. N2g3
Nd5 15. a3 Be7 16. Nc3 Be6 17. Re1 Bf6 18. Nge4 Nc7 19. Bxe6 Nxe6 20. d5 Bxc3
21. Nxc3 cxd5 22. Qxd5 1/2-1/2

";
            PgnParser parser = new PgnParser();
            parser.LoadPgn(pgn);
        }

        [Fact]
        public void WierdCase_PlayingVariation_NoRulesBroken()
        {
            string[] expected = new string[]
            {
                "r3kb1r/3n1pp1/p3p3/1P2q2p/3Q4/1P2P1N1/2bB1PPP/R3KB1R b KQkq - 0 17",
                "r3kb1r/3n1pp1/p3p3/1P2q3/3Q3p/1P2P1N1/2bB1PPP/R3KB1R w KQkq - 0 18",
                "r3kb1r/3n1pp1/p3p3/1P2q3/3Q3p/1P2P3/2bBNPPP/R3KB1R b KQkq - 1 18",
                "r3kb1r/3n1pp1/p3p3/1q6/3Q3p/1P2P3/2bBNPPP/R3KB1R w KQkq - 0 19",
                "r3kb1r/3n1pp1/p3p3/1q6/7p/1PQ1P3/2bBNPPP/R3KB1R b KQkq - 1 19",
                "r3kb1r/3n1pp1/p3p3/2q5/7p/1PQ1P3/2bBNPPP/R3KB1R w KQkq - 2 20",
                "r3kb1r/3n1pp1/p3p3/2Q5/7p/1P2P3/2bBNPPP/R3KB1R b KQkq - 0 20",
                "r3kb1r/5pp1/p3p3/2n5/7p/1P2P3/2bBNPPP/R3KB1R w KQkq - 0 21",
                "r3kb1r/5pp1/p3p3/2n5/3N3p/1P2P3/2bB1PPP/R3KB1R b KQkq - 1 21",
                "r3kb1r/5pp1/p3p3/8/3N3p/1n2P3/2bB1PPP/R3KB1R w KQkq - 0 22",
                "r3kb1r/5pp1/p3p3/8/7p/1n2P3/2NB1PPP/R3KB1R b KQkq - 0 22",
                "r3kb1r/5pp1/p3p3/8/7p/4P3/2NB1PPP/n3KB1R w Kkq - 0 23",
            };
            string[] moves = "a4b5 h5h4 g3e2 e5b5 d4c3 b5c5 c3c5 d7c5 e2d4 c5b3 d4c2 b3a1".Split(' ');
            Board b = new Board();
            b.Setup("r3kb1r/3n1pp1/p3p3/1p2q2p/P2Q4/1P2P1N1/2bB1PPP/R3KB1R w KQkq - 3 17");
            for (int i = 0; i < moves.Length; i++)
            {
                var move = Move.FromLan(moves[i]);
                b.DoMove(move);
                string actual = b.DumpFen();
                Assert.True(actual == expected[i]);
            }
            // make sure this move is available (and only once...)
            b.GenerateMoves().Single(x => x.Lan == "c2a1");
           
        }

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

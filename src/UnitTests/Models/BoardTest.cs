using ChessBucket.Models;
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
        public void GameWith51Rce6_Parsing_CompletesWithNoErrors()
        {
            string pgn = @"[Event ""It, Monaco""]
[Site ""It, Monaco""]
[Date ""1998.??.??""]
[EventDate ""?""]
[Round ""4""]
[Result ""0-1""]
[White ""Vassily Ivanchuk""]
[Black ""Anatoly Karpov""]
[ECO ""B12""]
[WhiteElo ""?""]
[BlackElo ""?""]
[PlyCount ""134""]

1. e4 c6 2. d4 d5 3. e5 Bf5 4. Nf3 e6 5. Be2 Ne7 6. O-O c5
7. dxc5 Nec6 8. Be3 Nd7 9. c4 dxc4 10. Na3 Bxc5 11. Bxc5 Nxc5
12. Nxc4 O-O 13. Qc1 Nd3 14. Qe3 Qd5 15. Nd6 Ndxe5 16. Rfd1
Nxf3+ 17. Bxf3 Qe5 18. Qxe5 Nxe5 19. Bxb7 Rab8 20. Rd2 Nc4
21. Nxc4 Rxb7 22. f3 g5 23. Kf2 Rc7 24. b3 Kg7 25. Rad1 Kf6
26. Rd6 Rb8 27. Ne3 Rc5 28. b4 Rcc8 29. Ra6 Rb7 30. a4 Bg6
31. b5 Kg7 32. Rd4 Rc3 33. h4 h6 34. hxg5 hxg5 35. Nc4 Rc2+
36. Kg3 Kh6 37. Rc6 Ra2 38. Nd6 Re7 39. Rdc4 f5 40. Nc8 f4+
41. Kh2 Rh7 42. Rxe6 Kg7+ 43. Kg1 Ra1+ 44. Kf2 Rhh1 45. Rc7+
Kh6 46. Rcc6 Ra2+ 47. Re2 Raa1 48. Ree6 Kh5 49. Nxa7 Ra2+
50. Re2 Bd3 51. Rce6 Bxe2 52. Rxe2 Rxa4 53. Nc6 Raa1 54. Re7
Ra2+ 55. Re2 Rxe2+ 56. Kxe2 Kh4 57. Kd3 Rb1 58. Kc4 Kg3
59. Nb4 Kxg2 60. b6 Rc1+ 61. Kd5 Rc8 62. Ke4 Re8+ 63. Kf5 Kxf3
64. Kxg5 Kg3 65. Nd3 f3 66. Kf5 f2 67. Nxf2 Kxf2 0-1
";
            PgnParser parser = new PgnParser();
            parser.LoadPgn(pgn);
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
        public void SampleGame_CastlingLong_IsAllowed()
        {
            string pgn = @"[Event ""FICS rated blitz game""]
[Site ""FICS freechess.org""]
[FICSGamesDBGameNo ""394926114""]
[White ""sharepointme""]
[Black ""Polymorphe""]
[WhiteElo ""1704""]
[BlackElo ""1689""]
[WhiteRD ""na""]
[BlackRD ""na""]
[TimeControl ""180+0""]
[Date ""2016.05.10""]
[Time ""11:30:00""]
[WhiteClock ""0:03:00.000""]
[BlackClock ""0:03:00.000""]
[ECO ""A40""]
[PlyCount ""81""]
[Result ""1-0""]

1. d4 e6 2. Bf4 c5 3. Nf3 cxd4 4. e3 dxe3 5. Bxe3 Nc6 6. c3 Nf6 7. Be2 Qc7 8. O-O h5 9. Nbd2 Ng4 10. Nc4 b5 11. Ncd2 Bd6 12. h3 a6 13. Ne4 Bh2+ 14. Kh1 f5 15. Neg5 Nce5 16. Qd4 Nxf3 17. Bxf3 Bg1 18. g3 Bb7 19. Bxb7 Qxb7+ 20. Kxg1 Nxe3 21. fxe3 h4 22. Qxg7 O-O-O 23. Qd4 hxg3 24. Qc5+ Kb8 25. Nf7 Rxh3 26. e4 d6 27. Nxd6 Rxd6 28. Qxd6+ Ka8 29. Rad1 Rh1+ 30. Kxh1 Qxe4+ 31. Kg1 Qe3+ 32. Kg2 Qe2+ 33. Kxg3 Qg4+ 34. Kf2 Qh4+ 35. Ke3 Qe4+ 36. Kd2 Qd5+ 37. Qxd5+ exd5 38. Rxf5 Kb7 39. Rg1 Kc6 40. Rf6+ Kc5 41. Rg8 {Black forfeits on time} 1-0
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
                output.WriteLine($"game: {i}");
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//0=. , 1=prnbqk, 7=prnbqk
namespace DemoSite.Models
{
    public class Board
    {
        private readonly int[] _board = new int[64];
        private bool _isInverted;

        public bool WhiteCanCastleShort;
        public bool WhiteCanCastleLong;
        public bool BlackCanCastleShort;
        public bool BlackCanCastleLong;
        public bool WhitesTurn { get; set; }
        public int EnpassantIndex { get; set; }
        //This is the number of halfmoves since the last capture or pawn advance.This is used to determine if a draw can be claimed under the fifty-move rule.
        public int HalfmoveClock { get; set; }
        //The number of the full move. It starts at 1, and is incremented after Black's move.        
        public int FullmoveNumber { get; set; }

        public Board()
        {
            Init();
        }
        public bool IsMate
        {
            get
            {
                bool invert = !WhitesTurn;
                bool isMate = GenerateMoves().Length == 0;
                if (isMate)
                {
                    if (invert) Invert();
                    isMate = !KingIsSafe();
                    if (invert) Invert();
                }
                return isMate;
            }
        }
        public bool IsDraw
        {
            get
            {
                bool invert = !WhitesTurn;
                bool isDraw = GenerateMoves().Length == 0;
                if (isDraw)
                {
                    // no moves
                    if (invert) Invert();
                    isDraw = KingIsSafe();
                    if (invert) Invert();
                }
                else
                {
                    // technical draw
                    isDraw = IsTechnicalDraw();
                }
                return isDraw;
            }
        }

        public int[] PieceCount()
        {
            //0=. , 1=prnbqk, 7=prnbqk
            //13=wlb, 14=wdb, 15=blb, 16=bdb
            int[] count = new int[17];
            for (int i = 0; i < 64; i++)
            {
                count[_board[i]]++;
                if (i % 2 == 0 && _board[i] == 4) count[13]++;
                if (i % 2 == 1 && _board[i] == 4) count[14]++;
                if (i % 2 == 0 && _board[i] == 10) count[15]++;
                if (i % 2 == 1 && _board[i] == 10) count[16]++;
            }
            return count;
        }

        private bool IsTechnicalDraw()
        {
            int[] pieceCount = PieceCount();
            int whiteOther = pieceCount[1] + pieceCount[2] + pieceCount[5]; //P, R, Q
            int blackOther = pieceCount[7] + pieceCount[8] + pieceCount[11]; //p, r, q
            int whiteKnights = pieceCount[3];
            int whiteLightBishops = pieceCount[13];
            int whiteDarkBishops = pieceCount[14];
            int whiteBishops = whiteDarkBishops + whiteLightBishops;
            int blackKnights = pieceCount[9];
            int blackLightBishops = pieceCount[15];
            int blackDarkBishops = pieceCount[16];
            int blackBishops = blackDarkBishops + blackLightBishops;

            if (blackOther > 0 || whiteOther > 0) return false;
            //king versus king
            if (blackBishops == 0 && blackKnights == 0 && whiteBishops == 0 && whiteKnights == 0) return true;
            //king and bishop versus king
            if (blackBishops == 1 && blackKnights == 0 && whiteBishops == 0 && whiteKnights == 0) return true;
            if (blackBishops == 0 && blackKnights == 0 && whiteBishops == 1 && whiteKnights == 0) return true;
            //king and knight versus king
            if (blackBishops == 0 && blackKnights == 1 && whiteBishops == 0 && whiteKnights == 0) return true;
            if (blackBishops == 0 && blackKnights == 0 && whiteBishops == 0 && whiteKnights == 1) return true;
            //king and bishop versus king and bishop with the bishops on the same colour. (Any number of additional bishops of either color on the same color of square due to underpromotion do not affect the situation.)
            if (blackKnights == 0 && whiteKnights == 0 && whiteDarkBishops == 0 && blackDarkBishops == 0) return true;
            if (blackKnights == 0 && whiteKnights == 0 && whiteLightBishops == 0 && blackLightBishops == 0) return true;

            return false;
        }
        public void DoMove(Move move)
        {
            HalfmoveClock++;
            bool invert = !WhitesTurn;
            if (invert)
            {
                Invert();
                move.Invert();
                WhitesTurn = true;
            }
            // do move as white
            bool isCapture = _board[move.ToNumber] != 0;
            bool isPawnMove = _board[move.FromNumber] == 1;
            if (isCapture || isPawnMove) HalfmoveClock = 0;
            if (_isInverted) FullmoveNumber++;
            DoMoveAsWhite(move);
            if (invert)
            {
                Invert();
                move.Invert();
                WhitesTurn = false;
            }
            WhitesTurn = !WhitesTurn;
        }

        private void Init()
        {
            FullmoveNumber = 1;
            HalfmoveClock = 0;
            EnpassantIndex = 0;
            WhitesTurn = true;
            WhiteCanCastleShort = false;
            WhiteCanCastleLong = false;
            BlackCanCastleShort = false;
            BlackCanCastleLong = false;
            for (int i = 0; i < 64; i++)
            {
                _board[i] = 0;
            }
        }

        public void StartPosition()
        {
            Init();
            for (int i = 0; i < 8; i++)
            {
                _board[8 + i] = 1;
                _board[6 * 8 + i] = 7;
            }
            _board[0] = _board[7] = 2;
            _board[1] = _board[6] = 3;
            _board[2] = _board[5] = 4;
            _board[3] = 5;
            _board[4] = 6;
            _board[56] = _board[63] = 8;
            _board[57] = _board[62] = 9;
            _board[58] = _board[61] = 10;
            _board[59] = 11;
            _board[60] = 12;
            WhitesTurn = true;
            WhiteCanCastleShort = true;
            WhiteCanCastleLong = true;
            BlackCanCastleShort = true;
            BlackCanCastleLong = true;
        }
        public Move[] GenerateMoves()
        {
            bool invert = !WhitesTurn;
            if (invert)
            {
                Invert();
                WhitesTurn = true;
            }
            List<Move> moves = new List<Move>();
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i] == 0) continue;
                if (_board[i] == 1) moves.AddRange(LegalPawnMoves(i));
                if (_board[i] == 2) moves.AddRange(LegalRookMoves(i));
                if (_board[i] == 3) moves.AddRange(LegalKnightMoves(i));
                if (_board[i] == 4) moves.AddRange(LegalBishopMoves(i));
                if (_board[i] == 5) moves.AddRange(LegalQueenMoves(i));
                if (_board[i] == 6) moves.AddRange(LegalKingMoves(i));
            }
            if (invert)
            {
                Invert();
                moves.ForEach(m => m.Invert());
                WhitesTurn = false;
            }

            return moves.ToArray();
        }

        public string DumpFen()
        {
            StringBuilder sb = new StringBuilder();
            string output = ".PRNBQKprnbqk";
            for (int r = 7; r >= 0; r--)
            {
                int spaces = 0;
                for (int f = 0; f < 8; f++)
                {
                    int i = r * 8 + f;
                    char p = output[_board[i]];
                    if (p == '.')
                        spaces++;
                    if ((f == 7 || p != '.') && spaces > 0)
                    {
                        sb.Append(spaces);
                        spaces = 0;
                    }
                    if (p != '.')
                    {
                        sb.Append(p);
                        spaces = 0;
                    }
                }
                if (r != 0) sb.Append('/');
            }
            sb.Append(" " + (WhitesTurn ? 'w' : 'b') + " ");
            sb.Append(WhiteCanCastleShort ? "K" : "-");
            sb.Append(WhiteCanCastleLong ? "Q" : "-");
            sb.Append(BlackCanCastleShort ? "k" : "-");
            sb.Append(BlackCanCastleLong ? "q" : "-");
            string enpassantSquare = "" + (char)('a' + (EnpassantIndex % 8)) + (char)('1' + (EnpassantIndex / 8));
            sb.Append(" " + (EnpassantIndex == 0 ? "-" : enpassantSquare));
            sb.Append(" " + HalfmoveClock);
            sb.Append(" " + FullmoveNumber);
            return sb.ToString();
        }

        public void Setup(string fen)
        {
            string pieces = ".PRNBQKprnbqk";

            string[] blocks = fen.Trim().Split(new char[] { ' ' });
            string boardInfo = blocks[0];
            boardInfo = boardInfo
               .Replace("/", "")
               .Replace("1", ".")
               .Replace("2", "..")
               .Replace("3", "...")
               .Replace("4", "....")
               .Replace("5", ".....")
               .Replace("6", "......")
               .Replace("7", ".......")
               .Replace("8", "........");
            int i = 0;
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    char pieceCh = boardInfo[i++];
                    int boardIndex = rank * 8 + file;
                    int piece = pieces.IndexOf(pieceCh);
                    _board[boardIndex] = piece;
                }
            }
            WhitesTurn = blocks[1] == "w";
            WhiteCanCastleShort = blocks[2][0] == 'K';
            WhiteCanCastleLong = blocks[2][1] == 'Q';
            BlackCanCastleShort = blocks[2][2] == 'k';
            BlackCanCastleLong = blocks[2][3] == 'q';
            EnpassantIndex = blocks[3] == "-" ? 0 : (char.ToUpper(blocks[3][0]) - 'A') + 8 * (blocks[3][1] - '1');
            HalfmoveClock = int.Parse(blocks[4]);
            FullmoveNumber = int.Parse(blocks[5]);


        }


        public void DumpHtml(string movetext, Move[] moves)
        {
            StringBuilder sb = new StringBuilder();
            string output = ".PRNBQKprnbqk";
            for (int r = 7; r >= 0; r--)
            {
                for (int f = 0; f < 8; f++)
                {
                    int i = r * 8 + f;
                    if (_board[i] != 0)
                    {
                        sb.Append("" + (char)('a' + f) + (char)('1' + r) + ": '");
                        char l = output[_board[i]];
                        sb.Append(char.IsLower(l) ? "b" : "w");
                        sb.AppendLine(char.ToUpper(l) + "',");
                    }
                }
            }
            string text = File.ReadAllText("TextFile1.txt");
            StringBuilder moveHtml = new StringBuilder();
            for (int i = 0; i < moves.Length; i++)
            {
                moveHtml.AppendLine(moves[i].Text + "<br/>");
            }

            File.WriteAllText("board.html",
                text.Replace(@"{{0}}",
                sb.ToString())
                    .Replace(@"{{1}}", movetext)
                    .Replace(@"{{2}}",
                moveHtml.ToString()));
        }

        public string DumpAscii()
        {
            StringBuilder sb = new StringBuilder();
            string output = ".PRNBQKprnbqk";
            for (int r = 7; r >= 0; r--)
            {
                sb.AppendLine();
                for (int f = 0; f < 8; f++)
                {
                    int i = r * 8 + f;
                    sb.Append(output[_board[i]]);
                }
            }
            return sb.ToString();
        }
        private void Invert()
        {
            for (int i = 0; i < 32; i++)
            {
                var square1 = _board[63 - i] - 6;
                var square2 = _board[i] - 6;
                if (square1 == -6) square1 = 0;
                else if (square1 <= 0) square1 += 12;
                if (square2 == -6) square2 = 0;
                else if (square2 <= 0) square2 += 12;
                _board[63 - i] = square2;
                _board[i] = square1;
            }
            if (EnpassantIndex > 0)
                EnpassantIndex = 63 - EnpassantIndex;
            bool tempShort = WhiteCanCastleShort;
            bool tempLong = WhiteCanCastleLong;
            WhiteCanCastleShort = BlackCanCastleShort;
            WhiteCanCastleLong = BlackCanCastleLong;
            BlackCanCastleShort = tempShort;
            BlackCanCastleLong = tempLong;
            _isInverted = !_isInverted;
        }
        private void PseudoLegalMoves(List<Move> all, int src, int[] dst)
        {
            foreach (var q in dst)
            {
                if (_board[q] >= 1 && _board[q] <= 6)
                    break;
                all.Add(new Move(src, q));
                if (_board[q] >= 7)
                    break;
            }
        }

        private Move[] LegalPawnMoves(int s)
        {
            List<Move> all = new List<Move>();
            bool rank7 = s / 8 == 6;
            bool rank2 = s / 8 == 1;
            bool rank5 = s / 8 == 4;
            bool canMoveOneSquare = _board[s + 8] == 0;
            if (canMoveOneSquare)
            {
                if (rank7)
                    all.AddRange(new[] { new Move(s, s + 8, 2), new Move(s, s + 8, 3), new Move(s, s + 8, 4), new Move(s, s + 8, 5) });
                else
                    all.Add(new Move(s, s + 8));
                if (rank2 && _board[s + 16] == 0)
                    all.Add(new Move(s, s + 16));
            }
            List<int> attackSquares = new List<int>();
            attackSquares.AddRange(Range(s, 1, 1, 1));
            attackSquares.AddRange(Range(s, -1, 1, 1));
            foreach (var attack in attackSquares)
            {
                if (rank7 && _board[attack] >= 7)
                    all.AddRange(new[] { new Move(s, attack, 2), new Move(s, attack, 3), new Move(s, attack, 4), new Move(s, attack, 5) });
                else if (rank5 && _board[attack] == 0 && (EnpassantIndex == attack - 8))
                    all.Add(new Move(s, attack) { EnpassantCapture = true });
                else if (!rank7 && _board[attack] >= 7)
                    all.Add(new Move(s, attack));
            }

            return all.Where(m => IsLegalMove(m)).ToArray();
        }
        private Move[] LegalRookMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 0));
            PseudoLegalMoves(all, s, Range(s, -1, 0));
            PseudoLegalMoves(all, s, Range(s, 0, 1));
            PseudoLegalMoves(all, s, Range(s, 0, -1));
            return all.Where(m => IsLegalMove(m)).ToArray();
        }

        private Move[] LegalKnightMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 2, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 2, -1, 1));
            PseudoLegalMoves(all, s, Range(s, -2, 1, 1));
            PseudoLegalMoves(all, s, Range(s, -2, -1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, 2, 1));
            PseudoLegalMoves(all, s, Range(s, 1, -2, 1));
            PseudoLegalMoves(all, s, Range(s, -1, 2, 1));
            PseudoLegalMoves(all, s, Range(s, -1, -2, 1));
            return all.Where(m => IsLegalMove(m)).ToArray();
        }
        private Move[] LegalBishopMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, -1));
            PseudoLegalMoves(all, s, Range(s, -1, 1));
            PseudoLegalMoves(all, s, Range(s, -1, -1));
            return all.Where(m => IsLegalMove(m)).ToArray();
        }
        private Move[] LegalQueenMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, -1));
            PseudoLegalMoves(all, s, Range(s, -1, 1));
            PseudoLegalMoves(all, s, Range(s, -1, -1));
            PseudoLegalMoves(all, s, Range(s, 1, 0));
            PseudoLegalMoves(all, s, Range(s, -1, 0));
            PseudoLegalMoves(all, s, Range(s, 0, 1));
            PseudoLegalMoves(all, s, Range(s, 0, -1));
            return all.Where(m => IsLegalMove(m)).ToArray();
        }
        private Move[] LegalKingMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, -1, 1));
            PseudoLegalMoves(all, s, Range(s, -1, 1, 1));
            PseudoLegalMoves(all, s, Range(s, -1, -1, 1));
            PseudoLegalMoves(all, s, Range(s, 0, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 0, -1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, 0, 1));
            PseudoLegalMoves(all, s, Range(s, -1, 0, 1));
            List<Move> legalMoves = all.Where(m => IsLegalMove(m)).ToList();
            if (!KingIsSafe() || legalMoves.Count == 0)
                return legalMoves.ToArray();
            // handle castle moves
            bool eastEmpty = Range(s, 1, 0, 2).Where(i => _board[i] == 0).Count() == 2;
            var m1 = new Move(s, s + 1);
            var m2 = new Move(s, s + 2);
            bool castleRight = _isInverted ? WhiteCanCastleLong : WhiteCanCastleShort;
            bool castleLeft = _isInverted ? WhiteCanCastleShort : WhiteCanCastleLong;
            if (castleRight && eastEmpty && IsLegalMove(m1) && IsLegalMove(m2))
                legalMoves.Add(m2);
            bool westEmpty = Range(s, -1, 0, 2).Where(i => _board[i] == 0).Count() == 2;
            m1 = new Move(s, s - 1);
            m2 = new Move(s, s - 2);
            if (castleLeft && westEmpty && IsLegalMove(m1) && IsLegalMove(m2))
                legalMoves.Add(m2);
            return legalMoves.ToArray();
        }
        private UndoAsWhite DoMoveAsWhite(Move move)
        {
            // setup undo
            UndoAsWhite undo = new UndoAsWhite();
            undo.FromNumber = move.FromNumber;
            undo.ToNumber = move.ToNumber;
            undo.EnpassentIndex = EnpassantIndex;
            undo.EnpassantCapture = move.EnpassantCapture;
            undo.BoardFrom = _board[move.FromNumber];
            undo.BoardTo = _board[move.ToNumber];
            undo.WhiteCanCastleLong = WhiteCanCastleLong;
            undo.WhiteCanCastleShort = WhiteCanCastleShort;
            // do move
            if (move.IsPromoting)
                _board[move.ToNumber] = move.PromoteTo;
            else
                _board[move.ToNumber] = _board[move.FromNumber];
            _board[move.FromNumber] = 0;
            bool kingMove = _board[move.ToNumber] == 6;
            int kingSpeed = kingMove ? move.ToNumber - move.FromNumber : 0;
            // castle right
            if (kingSpeed == 2)
            {
                _board[7] = 0;
                _board[move.ToNumber - 1] = 2;
            }
            // castle left
            if (kingSpeed == -2)
            {
                _board[0] = 0;
                _board[move.ToNumber + 1] = 2;
            }
            // no castling after king/rook moves
            if (kingMove)
            {
                WhiteCanCastleLong = false;
                WhiteCanCastleShort = false;
            }
            bool leftRookMove = _board[move.ToNumber] == 2 && move.FromNumber == 0;
            if (leftRookMove && _isInverted) WhiteCanCastleShort = false;
            if (leftRookMove && !_isInverted) WhiteCanCastleLong = false;
            bool rightRookMove = _board[move.ToNumber] == 2 && move.FromNumber == 7;
            if (rightRookMove && _isInverted) WhiteCanCastleLong = false;
            if (rightRookMove && !_isInverted) WhiteCanCastleShort = false;

            // enpassant
            if (move.EnpassantCapture)
                _board[move.ToNumber - 8] = 0;
            // pawn moves two ranks
            if (_board[move.ToNumber] == 1 && (move.ToNumber - move.FromNumber == 16))
                EnpassantIndex = move.ToNumber;
            else
                EnpassantIndex = 0;
            return undo;
        }
        private void UndoMoveAsWhite(UndoAsWhite undoMove)
        {
            _board[undoMove.FromNumber] = undoMove.BoardFrom;
            _board[undoMove.ToNumber] = undoMove.BoardTo;
            if (undoMove.EnpassantCapture)
                _board[undoMove.ToNumber - 8] = 7;
            bool kingMove = _board[undoMove.FromNumber] == 6;
            int kingSpeed = kingMove ? undoMove.ToNumber - undoMove.FromNumber : 0;
            // undo castle short?
            if (kingSpeed == 2)
            {
                _board[7] = 2;
                _board[undoMove.ToNumber - 1] = 0;
            }
            // castle left
            if (kingSpeed == -2)
            {
                _board[0] = 2;
                _board[undoMove.ToNumber + 1] = 0;
            }
            EnpassantIndex = undoMove.EnpassentIndex;
            WhiteCanCastleShort = undoMove.WhiteCanCastleShort;
            WhiteCanCastleLong = undoMove.WhiteCanCastleLong;
        }
        private bool IsLegalMove(Move move)
        {
            UndoAsWhite undo = DoMoveAsWhite(move);
            bool res = KingIsSafe();
            UndoMoveAsWhite(undo);
            return res;
        }
        private bool KingIsSafe()
        {
            int kingIndex = Array.IndexOf(_board, 6);
            bool firstMove;
            firstMove = true;
            foreach (var i in Range(kingIndex, 0, 1))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 8 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, 1, 1))
            {
                if (firstMove && (_board[i] == 12 || _board[i] == 7)) return false;
                if (_board[i] == 10 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, -1, 1))
            {
                if (firstMove && (_board[i] == 12 || _board[i] == 7)) return false;
                if (_board[i] == 10 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, 1, 0))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 8 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, -1, 0))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 8 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, 0, -1))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 8 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, 1, -1))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 10 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            firstMove = true;
            foreach (var i in Range(kingIndex, -1, -1))
            {
                if (firstMove && _board[i] == 12) return false;
                if (_board[i] == 10 || _board[i] == 11) return false;
                if (_board[i] != 0) break;
                firstMove = false;
            }
            List<int> n = new List<int>();
            n.AddRange(Range(kingIndex, 2, 1, 1));
            n.AddRange(Range(kingIndex, 2, -1, 1));
            n.AddRange(Range(kingIndex, -2, 1, 1));
            n.AddRange(Range(kingIndex, -2, -1, 1));
            n.AddRange(Range(kingIndex, 1, 2, 1));
            n.AddRange(Range(kingIndex, 1, -2, 1));
            n.AddRange(Range(kingIndex, -1, 2, 1));
            n.AddRange(Range(kingIndex, -1, -2, 1));
            foreach (var q in n)
            {
                if (_board[q] == 9) return false;
            }
            return true;
        }
        private int[] Range(int i, int dx, int dy, int limit = 8)
        {
            int x = i % 8;
            int y = i / 8;
            List<int> all = new List<int>();
            while (limit-- > 0)
            {
                x += dx;
                y += dy;
                if (x < 0 || x > 7 || y < 0 || y > 7) break;
                i = y * 8 + x;
                all.Add(i);
            }
            return all.ToArray();
        }


        public void PopulateSan(Move[] moves)
        {
            string pieces = ".PRNBQKPRNBQK";
            foreach (var m in moves)
            {
                char piece = pieces[_board[m.FromNumber]];
                int moveSpeed = Math.Abs(m.FromNumber - m.ToNumber);
                if (piece == 'K' && moveSpeed == 2)
                {
                    m.San = (m.ToNumber == 2 || m.ToNumber == 58) ? "o-o-o" : "o-o";
                    continue;
                }
                string destinationInfo = m.Text.Substring(2);
                string promotionInfo = "";
                if (m.IsPromoting) promotionInfo = "=" + pieces[m.PromoteTo];
                bool capture = _board[m.ToNumber] != 0 || m.EnpassantCapture;
                string captureInfo = capture ? "x" : "";
                string pieceInfo;
                if (capture && piece == 'P')
                    pieceInfo = "" + m.Text[0];
                else
                    pieceInfo = piece != 'P' ? "" + piece : "";
                m.San = string.Format("{0}{1}{2}{3}", pieceInfo, captureInfo, destinationInfo, promotionInfo);
            }

            // make unique
            var z = moves
                .GroupBy(c => c.San)
                .Where(grp => grp.Count() > 1)
                .Select(grp => new { list = grp.ToList() });
            foreach (var grp in z.ToList())
            {
                foreach (var m in grp.list)
                {
                    if (grp.list.Any(x => x != m && m.Text[0] == x.Text[0]))
                    {
                        if (grp.list.Any(x => x != m && m.Text[1] == x.Text[1]))
                            m.San = m.San.Insert(1, m.Text.Substring(0, 2));
                        else
                            m.San = m.San.Insert(1, "" + m.Text[1]);
                    }
                    else
                        m.San = m.San.Insert(1, "" + m.Text[0]);
                }
            }

        }
    }
    public class Move
    {
        private const string pieceCodes = ".prnbqkprnbqk";
        public int FromNumber { get; set; }
        public int ToNumber { get; set; }
        public bool EnpassantCapture { get; set; }
        public int PromoteTo { get; set; }
        public bool IsPromoting
        {
            get { return PromoteTo != 0; }
        }
        public Move(int src, int dst)
        {
            FromNumber = src;
            ToNumber = dst;
        }
        public Move(int src, int dst, int p) : this(src, dst)
        {
            PromoteTo = p;
        }
        public string Text
        {
            get { return itoa(FromNumber) + itoa(ToNumber) + (IsPromoting ? "" + pieceCodes[PromoteTo] : ""); }
        }

        public string San { get; set; }

        private string itoa(int i)
        {
            int x = i % 8;
            int y = i / 8;
            string a = "" + (char)('a' + x) + (char)('1' + y);
            return a;
        }
        public void Invert()
        {
            FromNumber = 63 - FromNumber;
            ToNumber = 63 - ToNumber;
        }

        public static Move FromText(string move)
        {
            move = move.Trim().ToUpper();
            int src = (move[0] - 'A') + (move[1] - '1') * 8;
            int dst = (move[2] - 'A') + (move[3] - '1') * 8;
            string promoteChars = "..RNBQ";
            int p = move.Length < 5 ? 0 : promoteChars.IndexOf(move[4]);
            return new Move(src, dst, p);
        }
    }
    public class UndoAsWhite
    {
        public bool EnpassantCapture { get; internal set; }
        public int FromNumber { get; set; }
        public int ToNumber { get; internal set; }
        public int EnpassentIndex { get; internal set; }
        public int BoardFrom { get; internal set; }
        public int BoardTo { get; internal set; }
        public bool WhiteCanCastleLong { get; internal set; }
        public bool WhiteCanCastleShort { get; internal set; }

        public void Invert()
        {
            FromNumber = 63 - FromNumber;
            ToNumber = 63 - ToNumber;
        }
    }
}

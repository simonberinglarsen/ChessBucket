using System;
using System.Collections.Generic;
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
        public int EnpassantSquare { get; set; }
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
                bool blacksTurn = !WhitesTurn;
                bool isDraw = GenerateMoves().Length == 0;
                if (isDraw)
                {
                    // no moves
                    if (blacksTurn) Invert();
                    isDraw = KingIsSafe();
                    if (blacksTurn) Invert();
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
            bool blacksTurn = !WhitesTurn;
            if (blacksTurn)
            {
                Invert();
                move.Invert();
                WhitesTurn = true;
            }
            // do move as white
            bool isCapture = _board[move.ToSquare] != 0;
            bool isPawnMove = _board[move.FromSquare] == 1;
            if (isCapture || isPawnMove) HalfmoveClock = 0;
            if (_isInverted) FullmoveNumber++;
            DoMoveAsWhite(move);
            if (blacksTurn)
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
            EnpassantSquare = 0;
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
            sb.Append(WhiteCanCastleShort ? "K" : "");
            sb.Append(WhiteCanCastleLong ? "Q" : "");
            sb.Append(BlackCanCastleShort ? "k" : "");
            sb.Append(BlackCanCastleLong ? "q" : "");
            sb.Append(!BlackCanCastleLong && !BlackCanCastleShort && !WhiteCanCastleLong && !WhiteCanCastleShort ? "-" : "");

            string enpassantSquare = "" + (char)('a' + (EnpassantSquare % 8)) + (char)('1' + (EnpassantSquare / 8));
            sb.Append(" " + (EnpassantSquare == 0 ? "-" : enpassantSquare));
            sb.Append(" " + HalfmoveClock);
            sb.Append(" " + FullmoveNumber);
            return sb.ToString();
        }

        public void Setup(string fen)
        {
            string pieces = ".PRNBQKprnbqk";

            string[] blocks = fen.Trim().Split(' ');
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
            WhiteCanCastleShort = blocks[2].Contains('K');
            WhiteCanCastleLong = blocks[2].Contains('Q');
            BlackCanCastleShort = blocks[2].Contains('k');
            BlackCanCastleLong = blocks[2].Contains('q');
            EnpassantSquare = blocks[3] == "-" ? 0 : (char.ToUpper(blocks[3][0]) - 'A') + 8 * (blocks[3][1] - '1');
            HalfmoveClock = int.Parse(blocks[4]);
            FullmoveNumber = int.Parse(blocks[5]);
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
            if (EnpassantSquare > 0)
                EnpassantSquare = 63 - EnpassantSquare;
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
                else if (rank5 && _board[attack] == 0 && (EnpassantSquare == attack))
                    all.Add(new Move(s, attack));
                else if (!rank7 && _board[attack] >= 7)
                    all.Add(new Move(s, attack));
            }

            return all.Where(IsLegalMove).ToArray();
        }
        private Move[] LegalRookMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 0));
            PseudoLegalMoves(all, s, Range(s, -1, 0));
            PseudoLegalMoves(all, s, Range(s, 0, 1));
            PseudoLegalMoves(all, s, Range(s, 0, -1));
            return all.Where(IsLegalMove).ToArray();
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
            return all.Where(IsLegalMove).ToArray();
        }
        private Move[] LegalBishopMoves(int s)
        {
            List<Move> all = new List<Move>();
            PseudoLegalMoves(all, s, Range(s, 1, 1));
            PseudoLegalMoves(all, s, Range(s, 1, -1));
            PseudoLegalMoves(all, s, Range(s, -1, 1));
            PseudoLegalMoves(all, s, Range(s, -1, -1));
            return all.Where(IsLegalMove).ToArray();
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
            return all.Where(IsLegalMove).ToArray();
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
            List<Move> legalMoves = all.Where(IsLegalMove).ToList();
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
            UndoAsWhite undo = new UndoAsWhite
            {
                FromSquare = move.FromSquare,
                ToSquare = move.ToSquare,
                EnpassantSquare = EnpassantSquare,
                OriginalPieceOnFromSquare = _board[move.FromSquare],
                OriginalPieceOnToSquare = _board[move.ToSquare],
                WhiteCanCastleLong = WhiteCanCastleLong,
                WhiteCanCastleShort = WhiteCanCastleShort
            };
            // do move
            if (move.IsPromoting)
                _board[move.ToSquare] = move.PromoteTo;
            else
                _board[move.ToSquare] = _board[move.FromSquare];
            _board[move.FromSquare] = 0;
            bool kingMove = _board[move.ToSquare] == 6;
            int kingSpeed = kingMove ? move.ToSquare - move.FromSquare : 0;
            // castle right
            if (kingSpeed == 2)
            {
                _board[7] = 0;
                _board[move.ToSquare - 1] = 2;
            }
            // castle left
            if (kingSpeed == -2)
            {
                _board[0] = 0;
                _board[move.ToSquare + 1] = 2;
            }
            // no castling after king/rook moves
            if (kingMove)
            {
                WhiteCanCastleLong = false;
                WhiteCanCastleShort = false;
            }
            if (_isInverted)
            {
                if (_board[0] != 2) WhiteCanCastleShort = false;
                if (_board[7] != 2) WhiteCanCastleLong = false;
                if (_board[56] != 8) BlackCanCastleShort = false;
                if (_board[63] != 8) BlackCanCastleLong= false;
            }
            else
            {
                if (_board[0] != 2) WhiteCanCastleLong = false;
                if (_board[7] != 2) WhiteCanCastleShort = false;
                if (_board[56] != 8) BlackCanCastleLong = false;
                if (_board[63] != 8) BlackCanCastleShort= false;
            }
            bool leftRookMoved = _board[0] != 2;
            if (leftRookMoved && _isInverted) WhiteCanCastleShort = false;
            if (leftRookMoved && !_isInverted) WhiteCanCastleLong = false;
            bool rightRookMoved = _board[7] != 2;
            if (rightRookMoved && _isInverted) WhiteCanCastleLong = false;
            if (rightRookMoved && !_isInverted) WhiteCanCastleShort = false;

            // enpassant
            bool enpassantCapture = (move.ToSquare - move.FromSquare) % 8 != 0 && undo.OriginalPieceOnToSquare == 0 && undo.OriginalPieceOnFromSquare == 1;
            if (enpassantCapture)
                _board[move.ToSquare - 8] = 0;
            // pawn moves two ranks
            bool blackPawnToLeft = move.ToSquare % 8 != 0 && _board[move.ToSquare - 1] == 7;
            bool blackPawnToRight = move.ToSquare % 8 != 7 && _board[move.ToSquare + 1] == 7;
            bool pawnNextToThis = blackPawnToLeft || blackPawnToRight;
            if (_board[move.ToSquare] == 1 && (move.ToSquare - move.FromSquare == 16) && pawnNextToThis)
            {
                EnpassantSquare = move.ToSquare - 8;
            }
            else
                EnpassantSquare = 0;
            return undo;
        }
        private void UndoMoveAsWhite(UndoAsWhite undoMove)
        {
            _board[undoMove.FromSquare] = undoMove.OriginalPieceOnFromSquare;
            _board[undoMove.ToSquare] = undoMove.OriginalPieceOnToSquare;
            bool enpassantCapture = (undoMove.ToSquare - undoMove.FromSquare) % 8 != 0 && undoMove.OriginalPieceOnToSquare == 0 && undoMove.OriginalPieceOnFromSquare == 1;
            if (enpassantCapture)
                _board[undoMove.ToSquare - 8] = 7;
            bool kingMove = _board[undoMove.FromSquare] == 6;
            int kingSpeed = kingMove ? undoMove.ToSquare - undoMove.FromSquare : 0;
            // undo castle short?
            if (kingSpeed == 2)
            {
                _board[7] = 2;
                _board[undoMove.ToSquare - 1] = 0;
            }
            // castle left
            if (kingSpeed == -2)
            {
                _board[0] = 2;
                _board[undoMove.ToSquare + 1] = 0;
            }
            EnpassantSquare = undoMove.EnpassantSquare;
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
                char piece = pieces[_board[m.FromSquare]];
                int moveSpeed = Math.Abs(m.FromSquare - m.ToSquare);
                if (piece == 'K' && moveSpeed == 2)
                {
                    m.San = (m.ToSquare == 2 || m.ToSquare == 58) ? "O-O-O" : "O-O";
                    continue;
                }
                string destinationInfo = m.Lan.Substring(2,2);
                string promotionInfo = "";
                if (m.IsPromoting) promotionInfo = "=" + pieces[m.PromoteTo];
                bool enpassantCapture = Math.Abs(m.ToSquare - m.FromSquare) % 8 != 0 && _board[m.ToSquare] == 0 && piece=='P';
                bool capture = _board[m.ToSquare] != 0 || enpassantCapture;
                string captureInfo = capture ? "x" : "";
                string pieceInfo;
                if (capture && piece == 'P')
                    pieceInfo = "" + m.Lan[0];
                else
                    pieceInfo = piece != 'P' ? "" + piece : "";
                m.San = $"{pieceInfo}{captureInfo}{destinationInfo}{promotionInfo}";
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
                    if (grp.list.Any(x => x != m && m.Lan[0] == x.Lan[0]))
                    {
                        m.San = grp.list.Any(x => x != m && m.Lan[1] == x.Lan[1]) ? m.San.Insert(1, m.Lan.Substring(0, 2)) : m.San.Insert(1, "" + m.Lan[1]);
                    }
                    else
                        m.San = m.San.Insert(1, "" + m.Lan[0]);
                }
            }

        }

        public string ToSan(string moveLan)
        {
            var moves = GenerateMoves();
            PopulateSan(moves);
            string san;
            try
            {
                san = moves.Single(x => x.Lan == moveLan).San;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
            return san;
        }
    }
    public class Move
    {
        private const string PieceCodes = ".prnbqkprnbqk";
        public int FromSquare { get; set; }
        public int ToSquare { get; set; }
        public int PromoteTo { get; set; }
        public string Lan => SquareIndexToAlgebraicNotation(FromSquare) + SquareIndexToAlgebraicNotation(ToSquare) + (IsPromoting ? "" + PieceCodes[PromoteTo] : "");
        public string San { get; set; }
        public bool IsPromoting => PromoteTo != 0;

        public Move(int src, int dst)
        {
            FromSquare = src;
            ToSquare = dst;
        }
        public Move(int src, int dst, int p) : this(src, dst)
        {
            PromoteTo = p;
        }
        public void Invert()
        {
            FromSquare = 63 - FromSquare;
            ToSquare = 63 - ToSquare;
        }
        public static Move FromLan(string move)
        {
            move = move.Trim().ToUpper();
            int src = (move[0] - 'A') + (move[1] - '1') * 8;
            int dst = (move[2] - 'A') + (move[3] - '1') * 8;
            string promoteChars = "..RNBQ";
            int p = move.Length < 5 ? 0 : promoteChars.IndexOf(move[4]);
            return new Move(src, dst, p);
        }
        private string SquareIndexToAlgebraicNotation(int i)
        {
            int x = i % 8;
            int y = i / 8;
            string a = "" + (char)('a' + x) + (char)('1' + y);
            return a;
        }
    }
    public class UndoAsWhite
    {
        public int FromSquare { get; set; }
        public int ToSquare { get; set; }
        public int EnpassantSquare { get; set; }
        public int OriginalPieceOnFromSquare { get; set; }
        public int OriginalPieceOnToSquare { get; set; }
        public bool WhiteCanCastleLong { get; set; }
        public bool WhiteCanCastleShort { get; set; }
    }
}

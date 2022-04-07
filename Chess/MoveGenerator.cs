using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    // TODO : Implement absolutely pinned pieces

    internal class MoveGenerator
    {
        public Dictionary<Piece, ulong> WhiteAttacks { get; set; }
        public Dictionary<Piece, ulong> BlackAttacks { get; set; }
        public Dictionary<Piece, ulong> AllAttacks { get; set; }
        public Dictionary<Piece, ulong> HypotheticalWhitePawnAttacks { get; set; } // Hypothetical Pawn attacks, used to make sure the king can't move in to check
        public Dictionary<Piece, ulong> HypotheticalBlackPawnAttacks { get; set; }
        private Board Board { get; set; }

        public MoveGenerator(Board board)
        {
            Board = board;

            WhiteAttacks = new Dictionary<Piece, ulong>();
            BlackAttacks = new Dictionary<Piece, ulong>();

            AllAttacks = new Dictionary<Piece, ulong>();

            HypotheticalWhitePawnAttacks = new Dictionary<Piece, ulong>();
            HypotheticalBlackPawnAttacks = new Dictionary<Piece, ulong>();
        }

        /// <summary>
        /// Generate all legal moves for this board.
        /// </summary>
        /// <param name="Board"></param>
        public void GetAllAttacks()
        {
            foreach (Piece piece in Board.Pieces)
            {
                if (piece.Color == Color.White)
                {
                    if (piece.Type == PieceType.Pawn)
                    {
                        piece.Moveset = GetMovesForWhitePawn(piece.Square.Coordinate);
                        WhiteAttacks[piece] = piece.Moveset;
                        HypotheticalWhitePawnAttacks[piece] = GetAllAttacksForWhitePawn(piece.Square.Coordinate);
                        AllAttacks[piece] = piece.Moveset;
                    }
                    else
                    {
                        piece.Moveset = GenerateMovesForPiece(piece);
                        WhiteAttacks[piece] = piece.Moveset;
                        AllAttacks[piece] = piece.Moveset;
                    }
                }
                else
                {
                    if (piece.Type == PieceType.Pawn)
                    {
                        piece.Moveset = GetMovesForBlackPawn(piece.Square.Coordinate);
                        BlackAttacks[piece] = piece.Moveset;
                        HypotheticalBlackPawnAttacks[piece] = GetAllAttacksForBlackPawn(piece.Square.Coordinate);
                        AllAttacks[piece] = piece.Moveset;
                    }
                    else
                    {
                        piece.Moveset = GenerateMovesForPiece(piece);
                        BlackAttacks[piece] = piece.Moveset;
                        AllAttacks[piece] = piece.Moveset;
                    }
                }
            }
        }

        /// <summary>
        /// Generates all legal moves for the moving side on this turn.
        /// </summary>
        /// <param name="Board"></param>
        public void GenerateMoves()
        {
            foreach (Piece piece in Board.Pieces)
            {
                // Only generate moves for the pieces that can be moved on the current turn
                if (piece.Color == Board.Turn)
                {
                    GetMoves(piece);
                }
            }
        }

        public ulong GetMoves(Piece piece)
        {
            if (GetCheckingPiece() != null)
            {
                return GetOutOfCheckMoves(piece);
            }
            else
            {
                return GenerateMovesForPiece(piece);
            }
        }

        /// <summary>
        /// Get all legal moves that can get a player out of check. AKA the alternative move generator for check :)
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public ulong GetOutOfCheckMoves(Piece piece)
        {
            /* TODO  : Implement double check
             * 
             * RULES : In the case of a double check, ONLY king moves & taking moves are valid. */

            // Get the piece performing check, if any
            Piece? checkingPiece = GetCheckingPiece();
            
            ulong interposingMoves = 0; // If the piece ISN'T a king, but can move between the checking piece and the king, this will show those moves.

            // If a piece is putting the current king in check // TODO: This should never be null so this check might be unnecessary.
            if (checkingPiece != null)
            {
                // If the piece is not a king, see if it can move between the king and the piece giving check.
                if (piece.Type != PieceType.King)
                    interposingMoves = InterposingMoves(checkingPiece, piece); // Get interposing moves
                else
                    return GetMovesForKing(piece); // The king can make his regular moves out of check instead of interposing.

                // Check if this piece can capture the piece delivering check
                return TakeCheckingPiece(checkingPiece, piece) | interposingMoves;
            }

            else
                return 0;
        }

        /// <summary>
        /// Generates moves for a specific piece.
        /// </summary>
        /// <param name="piece">The piece to move.</param>
        /// <param name="Board">The currently playing board.</param>
        /// <returns>The valid moveset. If the moveset doesn't exist, returns 0.</returns>
        public ulong GenerateMovesForPiece(Piece piece)
        {   
            // Clear list of moves
            // TODO: Store all generated moves on their piece???
            piece.Moves.Clear();

            // Used to get and remove the moving pieces own pieces from its list of valid moves.
            string ownPieces = "";

            if (piece.Color == Color.White)
                ownPieces = Constants.bbWhitePieces;
            else
                ownPieces = Constants.bbBlackPieces;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                
                    switch (piece.Color)
                    {
                        // Pawns already have pseudo move sets in the form of Hypothetical[Color]PawnAttacks
                        case Color.White:
                            return GetMovesForWhitePawn(piece.Square.Coordinate);

                        case Color.Black:
                            return GetMovesForBlackPawn(piece.Square.Coordinate);

                        default:
                            return 0;
                    }
                
                case PieceType.Knight:
                    piece.PseudoMoveset = GetMovesForKnight(piece);
                    return piece.PseudoMoveset & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.King: // Kings also have their own form of pseudo movesets
                    return GetMovesForKing(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Rook:
                    piece.PseudoMoveset = GetMovesForRook(piece, true);
                    return GetMovesForRook(piece, false) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Bishop:
                    piece.PseudoMoveset = GetDiagonalMask(Board.CoordinateValue[piece.Square.Coordinate]) | GetAntiDiagonalMask(Board.CoordinateValue[piece.Square.Coordinate]);
                    return GetMovesForBishop(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Queen:
                    piece.PseudoMoveset = (GetDiagonalMask(Board.CoordinateValue[piece.Square.Coordinate]) | GetAntiDiagonalMask(Board.CoordinateValue[piece.Square.Coordinate])) | GetMovesForRook(piece, true);
                    return (GetMovesForBishop(piece) | GetMovesForRook(piece, false)) & ~BitBoards.BitBoardDict[ownPieces];

                default:
                    return 0;
            }
        }

        // TODO: En pessant
        // TODO: Promotion
        #region Pawn Move Generation

        public ulong GetMovesForWhitePawn(Coordinate from)
        {
            // Generates a bitboard with the pieces starting position.
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Pushes

            ulong pushOne = (startPos << 8) & BitBoards.BitBoardDict["SquaresEmpty"]; // The square directly in front of the pawn.
            ulong pushTwo = pushOne << 8 & BitBoards.BitBoardDict["SquaresEmpty"] & Constants.Rank4; // If the square directly in front of the pushOne square is on the 4th rank,
                                                                                                 // meaning the pawn is on its starting square, it can move also move to the
                                                                                                 // 2nd square ahead of it.

            ulong validPushes = pushOne | pushTwo; // A combination of all valid pushes

            // Attacks
            ulong validAttacks = GetAttacksForWhitePawn(from);

            ulong validMoves = validPushes | validAttacks; // Bitboard containing all possible moves for a pawn.

            return validMoves;
        }

        public ulong GetAttacksForWhitePawn(Coordinate from)
        {
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Attacks
            ulong attackRight = (startPos & Constants.EmptyFileH) << 7; // The square up and left from the pawn can be attacked, unless the pawn is on File A.
            ulong attackLeft = (startPos & Constants.EmptyFileA) << 9; // The square up and right from the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            ulong validAttacks = allAttacks & BitBoards.BitBoardDict["BlackPieces"]; // An attack is valid if it is possible for the pawn to attack that way, and if the attacked square has an opposing piece on it.

            return validAttacks;
        }

        public ulong GetAllAttacksForWhitePawn(Coordinate from)
        {
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Attacks
            ulong attackRight = (startPos & Constants.EmptyFileH) << 7; // The square up and left from the pawn can be attacked, unless the pawn is on File A.
            ulong attackLeft = (startPos & Constants.EmptyFileA) << 9; // The square up and right from the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            return allAttacks;
        }

        public ulong GetMovesForBlackPawn(Coordinate from)
        {
            // Generates a bitboard with the pieces starting position.
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Pushes

            ulong pushOne = (startPos >> 8) & BitBoards.BitBoardDict["SquaresEmpty"]; // The square directly ahead of the pawn.
            ulong pushTwo = pushOne >> 8 & BitBoards.BitBoardDict["SquaresEmpty"] & Constants.Rank5; // If the square directly below the pushOne square is on the 5th rank,
                                                                                                 // meaning the pawn is on its starting square, it can move also move to the
                                                                                                 // 2nd square below it.

            ulong validPushes = pushOne | pushTwo; // A combination of all valid pushes

            // Attacks
            ulong validAttacks = GetAttacksForBlackPawn(from);

            ulong validMoves = validPushes | validAttacks; // Bitboard containing all possible moves for a pawn.

            return validMoves;
        }

        public ulong GetAttacksForBlackPawn(Coordinate from)
        {
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Attacks
            ulong attackRight = (startPos & Constants.EmptyFileA) >> 7; // The square ahead and left of the pawn can be attacked, unless the pawn is on File H.
            ulong attackLeft = (startPos & Constants.EmptyFileH) >> 9; // The square ahead and right of the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            ulong validAttacks = allAttacks & BitBoards.BitBoardDict["WhitePieces"]; // An attack is valid if it is possible for the pawn to attack that way, and if the attacked square has an opposing piece on it.

            return validAttacks;
        }
        public ulong GetAllAttacksForBlackPawn(Coordinate from)
        {
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Attacks
            ulong attackRight = (startPos & Constants.EmptyFileA) >> 7; // The square ahead and left of the pawn can be attacked, unless the pawn is on File H.
            ulong attackLeft = (startPos & Constants.EmptyFileH) >> 9; // The square ahead and right of the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            return allAttacks;
        }

        #endregion

        #region Knight Move Generation

        public ulong GetMovesForKnight(Piece piece)
        {
            /* 

            Knight move naming visualized:

            -2-3-
            1-.-4
            -.K.-
            8-.-5
            -7-6-

             */

            Coordinate from = piece.Square.Coordinate;

            ulong startingPos = (ulong)1 << Board.CoordinateValue[from];

            // Eliminate certain knight move possibilities based on its starting location.
            ulong clip1And8 = Constants.EmptyFileA & Constants.EmptyFileB;
            ulong clip2And7 = Constants.EmptyFileA;
            ulong clip3And6 = Constants.EmptyFileH;
            ulong clip4And5 = Constants.EmptyFileH & Constants.EmptyFileG;

            // Each individual, potentially valid knight moves.
            ulong spot1 = (startingPos & clip1And8) << 10;
            ulong spot2 = (startingPos & clip2And7) << 17;
            ulong spot3 = (startingPos & clip3And6) << 15;
            ulong spot4 = (startingPos & clip4And5) << 6;

            ulong spot5 = (startingPos & clip4And5) >> 10;
            ulong spot6 = (startingPos & clip3And6) >> 17;
            ulong spot7 = (startingPos & clip2And7) >> 15;
            ulong spot8 = (startingPos & clip1And8) >> 6;

            // BitBoard containing all potentially valid knight moves at once.
            ulong knightValid = spot1 | spot2 | spot3 | spot4 |
                                spot5 | spot6 | spot7 | spot8;

            // Get name of color to pass to bitboard dictionary.
            string color = piece.Color.ToString();
            knightValid = knightValid & ~BitBoards.BitBoardDict[color + "Pieces"]; // Knight can only move to squares NOT containing pieces of its own color.

            return knightValid;
        }

        #endregion

        // TODO: Castling
        #region King Move Generation

        public ulong GetMovesForKing(Piece piece)
        {
            /* 
             
             King move naming visualized:
             123
             4K5
             678
             
             */

            // Get the opponents attackset, to make sure king doesn't move into check
            Dictionary<Piece, ulong> opponentAttacks = new Dictionary<Piece, ulong>();

            foreach (Piece p in (piece.Color == Color.White ? Board.Pieces.Where(p => p.Color == Color.Black) : Board.Pieces.Where(p => p.Color == Color.White)))
            {
                if (p.Type != PieceType.Pawn)
                    opponentAttacks[p] = p.PseudoMoveset;
            };
            
            foreach (var entry in piece.Color == Color.White ? HypotheticalBlackPawnAttacks : HypotheticalWhitePawnAttacks)
                opponentAttacks[entry.Key] = entry.Value;

            string opponentPieces = piece.Color == Color.White ? Constants.bbBlackPieces : Constants.bbWhitePieces;

            string color = piece.Color.ToString();
            ulong ownPieces = BitBoards.BitBoardDict[color + "Pieces"];

            // The kings starting position
            Coordinate from = piece.Square.Coordinate;

            // Bitboard representing the current position
            ulong startingPos = (ulong)1 << Board.CoordinateValue[from];

            // If the king is on the left or rightmost file, he can't move further left or right.
            ulong kingClipFileA = startingPos & Constants.EmptyFileA;
            ulong kingClipFileH = startingPos & Constants.EmptyFileH;

            // Generate moves
            ulong spot1 = kingClipFileA << 9;
            ulong spot2 = startingPos << 8;
            ulong spot3 = kingClipFileH << 7;
            ulong spot4 = kingClipFileA << 1;

            ulong spot5 = kingClipFileH >> 1;
            ulong spot6 = kingClipFileA >> 7;
            ulong spot7 = startingPos >> 8;
            ulong spot8 = kingClipFileH >> 9;

            ulong kingValid = spot1 | spot2 | spot3 | spot4 |
                              spot5 | spot6 | spot7 | spot8;

            // Generate castling moves
            switch (piece.Color)
            {
                case Color.White:
                    // Check that piece can castle to that side, and that the path between rook and king is clear.
                    if ((Board.EnPassantAndCastling.CanWhiteCastleShort()) && ((ownPieces | Constants.WhiteCastlingShortClear) == Constants.WhiteCastlingShortClear))
                        kingValid ^= Constants.WhiteCastlingShortMask;
                    if ((Board.EnPassantAndCastling.CanWhiteCastleLong()) && ((ownPieces | Constants.WhiteCastlingLongClear) == Constants.WhiteCastlingLongClear))
                        kingValid ^= Constants.WhiteCastlingLongMask;
                    break;
                case Color.Black:
                    if ((Board.EnPassantAndCastling.CanBlackCastleShort()) && ((ownPieces | Constants.BlackCastlingShortClear) == Constants.BlackCastlingShortClear))
                        kingValid ^= Constants.BlackCastlingShortMask;
                    if ((Board.EnPassantAndCastling.CanBlackCastleLong()) && ((ownPieces | Constants.BlackCastlingLongClear) == Constants.BlackCastlingLongClear))
                        kingValid ^= Constants.BlackCastlingLongMask;
                    break;
            }

            // Get name of color to pass to bitboard dictionary.
            kingValid = kingValid & ~ownPieces;


            Trace.WriteLine("---NEXT ATTEMPT---");

            // Remove squares from kings moveset, that would move the king into check // TODO: Implement faster function than dictionary
            foreach (var attackset in opponentAttacks)
            {
                if(attackset.Key.Square != null)
                {
                    Trace.WriteLine($"Attackset for {attackset.Key.Color} {attackset.Key.Type} on {attackset.Key.Square.Coordinate}:\n" + BitBoards.BinaryMatrix(attackset.Value));
                }

                // Remove moves into check, as well as moves that take an opponent piece if that moves king into check.
                // Because the potentially checking pieces attackset doesn't include squares with its own pieces,
                // those moves are otherwise not removed.
                kingValid ^= kingValid & (attackset.Value);                                          
            }

            // Remove all squares next to the opponent king from the kings legal moveset.
            kingValid ^= kingValid & GetPseudoLegalMovesForKing(Board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color != piece.Color));

            return kingValid;
        }

        /// <summary>
        /// Generates all pseudo legal moves for king. Used to mark all the blocks that the opponent king can legally move to, so that the moving king can't move next to him,
        /// because moving next to the opponent king is ALWAYS illegal, even if the opponent king couldn't "legally" take you on the next move.
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        private ulong GetPseudoLegalMovesForKing(Piece piece)
        {
            /* 
             
             King move naming visualized:
             123
             4K5
             678
             
             */

            // The kings starting position
            Coordinate from = piece.Square.Coordinate;

            // Bitboard representing the current position
            ulong startingPos = (ulong)1 << Board.CoordinateValue[from];

            // If the king is on the left or rightmost file, he can't move further left or right.
            ulong kingClipFileA = startingPos & Constants.EmptyFileA;
            ulong kingClipFileH = startingPos & Constants.EmptyFileH;

            // Generate moves
            ulong spot1 = kingClipFileA << 9;
            ulong spot2 = startingPos << 8;
            ulong spot3 = kingClipFileH << 7;
            ulong spot4 = kingClipFileA << 1;

            ulong spot5 = kingClipFileH >> 1;
            ulong spot6 = kingClipFileA >> 7;
            ulong spot7 = startingPos >> 8;
            ulong spot8 = kingClipFileH >> 9;

            ulong kingValid = spot1 | spot2 | spot3 | spot4 |
                              spot5 | spot6 | spot7 | spot8;

            // Get name of color to pass to bitboard dictionary.
            string color = piece.Color.ToString();

            return kingValid;
        }

        #endregion

        #region Rook Move Generation

        public ulong GetMovesForRook(Piece piece, bool isPseudoMoves)
        {
            // The rooks starting position
            Coordinate from = piece.Square.Coordinate;

            // The value of said square
            byte square = Board.CoordinateValue[from];

            // Rooks can move to all free squares directly north, east, south and west of its starting square.
            ulong rookValid = GetRankMoves(square, isPseudoMoves) | GetFileMoves(square, isPseudoMoves);
            return rookValid;
        }

        #endregion

        #region Bishop Move Generation

        public ulong GetMovesForBishop(Piece piece)
        {
            // The bishops starting position
            Coordinate from = piece.Square.Coordinate;

            // The value of that square
            byte square = Board.CoordinateValue[from];

            // Generates a bitboard with the pieces starting position.
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Get all diagonal and antidiagonal moves for whereever the bishop currently is.
            ulong bishopValid = GetDiagonalMoves(square, startPos) | GetAntiDiagonalMoves(square, startPos);

            return bishopValid;
        }

        #endregion

        #region Generate Positive And Negative Moves
        private ulong GetPositiveMoves(int rayDirection, byte square, bool isPseudoMoves)
        {
            ulong moves = Ray.Rays[rayDirection][square];
            ulong blocker = 0;

            blocker = moves & BitBoards.BitBoardDict[Constants.bbSquaresOccupied]; // moves on an empty board AND'ed with the current bitboard of all pieces.

            // if this direction is eventually blocked off by a piece.
            if (blocker != 0)
            {
                // using forward bitscan to find the least significant bit
                int blockerSquare = BitHelper.GetLeastSignificant1Bit(blocker);

                moves = moves ^ Ray.Rays[rayDirection][blockerSquare];

                if (isPseudoMoves)
                    moves = moves | (ulong)1 << blockerSquare;
                
            }

            return moves;
        }

        private ulong GetNegativeMoves(int rayDirection, byte square, bool isPseudoMoves)
        {
            ulong moves = Ray.Rays[rayDirection][square];
            ulong blocker = 0;

            blocker = moves & BitBoards.BitBoardDict[Constants.bbSquaresOccupied];

            // if this direction is eventually blocked off by a piece.
            if (blocker != 0)
            {
                // using forward bitscan to find the least significant bit
                int blockerSquare = BitHelper.GetMostSignificant1Bit(blocker);

                moves = moves ^ Ray.Rays[rayDirection][blockerSquare];

                if (isPseudoMoves)
                    moves = moves | (ulong)1 << blockerSquare;
                Trace.WriteLine($"Blocker square for {Board.SquareDict[Board.CoordinateValue.FirstOrDefault(c => c.Value == square).Key].Piece} on {Board.CoordinateValue.FirstOrDefault(c => c.Value == square).Key}: \n" + BitBoards.BinaryMatrix((ulong) 1 << blockerSquare));
            }

            return moves;
        }
        #endregion

        #region Generate rank & file moves

        /// <summary>
        /// Get all squares to the east/west of a given square.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private ulong GetRankMoves(byte square, bool isPseudoMoves)
        {
            ulong westMoves = GetPositiveMoves(Ray.West, square, isPseudoMoves);
            ulong eastMoves = GetNegativeMoves(Ray.East, square, isPseudoMoves);

            ulong moves = eastMoves | westMoves; 
            return moves;
        }

        /// <summary>
        /// Get all squares to the north/south of a given square.
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        private ulong GetFileMoves(byte square, bool isPseudoMoves)
        {
            ulong northMoves = GetPositiveMoves(Ray.North, square, isPseudoMoves);
            ulong southMoves = GetNegativeMoves(Ray.South, square, isPseudoMoves);

            ulong moves = northMoves | southMoves;
            return moves;
        }

        #endregion

        #region Generate (anti-)diagonal moves

        private ulong GetDiagonalMask(byte square)
        {
            ulong diagonal = Constants.Diagonal;
            int diag = (square & 7) - (square >> 3);
            return diag >= 0 ? diagonal >> diag * 8 : diagonal << -diag * 8;
        }
        private ulong GetDiagonalMoves(byte square, ulong startPos)
        {
            ulong maskEx = GetDiagonalMask(square) ^ startPos; // Diagonal mask excluding the square the piece is currently on

            ulong forward = BitBoards.BitBoardDict[Constants.bbSquaresOccupied] & maskEx;
            ulong reverse = BinaryPrimitives.ReverseEndianness(forward);
            forward -= startPos;
            reverse -= BinaryPrimitives.ReverseEndianness(startPos);
            forward ^= BinaryPrimitives.ReverseEndianness(reverse);
            forward &= maskEx;
            return forward;
        }

        private ulong GetAntiDiagonalMask(byte square)
        {
            ulong antiDiagonal = Constants.AntiDiagonal;
            int diag = 7 - (square & 7) - (square >> 3);
            return diag >= 0 ? antiDiagonal >> diag * 8 : antiDiagonal << -diag * 8;
        }

        private ulong GetAntiDiagonalMoves(byte square, ulong startPos)
        {
            ulong maskEx = GetAntiDiagonalMask(square) ^ startPos; // Diagonal mask excluding the square the piece is currently on

            ulong forward = BitBoards.BitBoardDict[Constants.bbSquaresOccupied] & maskEx;
            ulong reverse = BinaryPrimitives.ReverseEndianness(forward);
            forward -= startPos;
            reverse -= BinaryPrimitives.ReverseEndianness(startPos);
            forward ^= BinaryPrimitives.ReverseEndianness(reverse);
            forward &= maskEx;
            return forward;
        }

        #endregion

        // TODO: Currently only checks pseudo legality.
        #region Check legality of move

        /// <summary>
        /// Checks whether a proposed move is pseudo-legal.
        /// </summary>
        /// <param name="from">The starting square.</param>
        /// <param name="to">The proposed end square.</param>
        /// <returns>True if the move can be completed (pseudo-legally), otherwise false.</returns>
        public bool IsPseudoLegal(Square from, Square to)
        {
            ulong validMoves = GetMoves(from.Piece);

            ulong destinationBB = (ulong)1 << Board.CoordinateValue[to.Coordinate];
            ulong isValidMove = validMoves & destinationBB;

            return (isValidMove > 0);
        }

        #endregion

        #region Get which pieces are under attack

        // Is king under attack ? AKA check for check
        public Piece? GetCheckingPiece()
        {
            // Dictionary of attacks based on side
            Dictionary<Piece, ulong> opponentAttacks = new Dictionary<Piece, ulong>();

            // The square that we are checking whether is under attacks
            ulong attackedSquare;

            // Get turn & determine which king/attack set to check against
            if (Board.Turn == Color.White)
            {
                attackedSquare = BitBoards.BitBoardDict[Constants.bbWhiteKing];
                opponentAttacks = BlackAttacks;
            }
            else
            {
                attackedSquare = BitBoards.BitBoardDict[Constants.bbBlackKing];
                opponentAttacks = WhiteAttacks;
            }

            // Check for attacks by all piece types
            foreach (var p in opponentAttacks)
            {
                if ((attackedSquare & p.Value) != 0)
                {
                    return p.Key;
                }
            }
            
            // No checks have been found:  return false.
            return null;
        }

        // Checks if a piece is still checking the king after the next move, rendering the move invalid
        public bool IsPieceStillChecking()
        {
            // Dictionary of attacks based on side
            Dictionary<Piece, ulong> opponentAttacks = new Dictionary<Piece, ulong>();

            // The square that we are checking whether is under attacks
            ulong attackedSquare;

            // Get turn & determine which king/attack set to check against
            if (Board.Turn != Color.White)
            {
                attackedSquare = BitBoards.BitBoardDict[Constants.bbWhiteKing];
                opponentAttacks = BlackAttacks;
            }
            else
            {
                attackedSquare = BitBoards.BitBoardDict[Constants.bbBlackKing];
                opponentAttacks = WhiteAttacks;
            }

            // Check for attacks by all piece types
            foreach (var p in opponentAttacks)
            {
                if ((attackedSquare & p.Value) != 0)
                {
                    return true;
                }
            }

            // No checks have been found:  return false.
            return false;
        }

        #endregion

        #region Legal moves while in single check

        /// <summary>
        /// Gets moves that attacks the checking piece
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public ulong TakeCheckingPiece(Piece checkingPiece, Piece attacker)
        {
            // Get all attacks to ensure properly updated bitboards TODO: Optimize this
            // GetAllAttacks();

            // Dictionary of attacks based on side
            //Dictionary<Piece, ulong> opponentAttacks = new Dictionary<Piece, ulong>();

            // The square that we are checking whether is under attacks
            ulong attackedSquare;

            attackedSquare = (ulong)1 << Board.CoordinateValue[checkingPiece.Square.Coordinate];

            //// Get turn & determine which king/attack set to check against
            //if (checkingPiece.Color == Color.White)
            //{
            //    opponentAttacks = BlackAttacks;
            //}
            //else
            //{
            //    opponentAttacks = WhiteAttacks;
            //}

/*            ulong myMoves = opponentAttacks[attacker]; */// DEBUG: So I can immediately see the value of opponentAttacks[attacker] in the locals tab :)

            // Check for possible attacks by the piece attempting to move.
            if ((attackedSquare & attacker.Moveset) != 0)
                return attackedSquare & attacker.Moveset;

            // No attacks have been found:  moveset is empty.
            return 0;
        }

        // OBSOLETE:
        //   THE KING CAN NOW _ALWAYS_ ONLY MOVE TO SAFE SQUARES.

        ///// <summary>
        ///// Gets the moves that can get the king to safety
        ///// </summary>
        ///// <param name="attackedKing"></param>
        ///// <returns></returns>
        //public ulong MoveToSafety(Piece attackedKing)
        //{
        //    // Dictionary of attacks based on side
        //    Dictionary<Piece, ulong> opponentAttacks = new Dictionary<Piece, ulong>();

        //    // All the squares the king can normally move to
        //    ulong kingMoves = GenerateMovesForPiece(attackedKing);

        //    // Get turn & determine which attack set to check against
        //    if (Board.Turn == Color.White)
        //    {
        //        opponentAttacks = BlackAttacks;
        //    }
        //    else
        //    {
        //        opponentAttacks = WhiteAttacks;
        //    }

        //    // Check for attacks by all piece types
        //    foreach (var attack in opponentAttacks)
        //    {
        //        if ((kingMoves & attack.Value) != 0)
        //        {
        //            /* kingMoves ^= (kingMoves & attack.Value); /* VALID SQUARES = O
        //                                                        ILLEGAL SQUARES = X
        //                                                        KING = K
        //                                                        ROOK = R
        //                                                        KNIGHT = N
                                                        
        //                                                        Pseudolegal king moves:     Pseudolegal rook moves:     King moves & rook moves:
        //                                                        XXNX                        XXNO                        XXNX
        //                                                        OOOR                        OOOR                        OOOR
        //                                                        OKOX                        XKXO                        XKXX
                                                                
        //                                                        This is the opposite of what we want, so we reverse it by exlusive or'ing it with the kings original moveset:

        //                                                        Pseudolegal king moves:     King moves & rook moves:    Moves out of check (king moves XOR (king moves & rook moves))
        //                                                        XXNX                        XXNX                        XXNX
        //                                                        OOOR                        OOOR                        XXXR
        //                                                        OKOX                        XKXX                        OKOX
        //                                                     */
                                                              
        //        }
        //    }

        //    // Return the kings leftover legal moves
        //    return kingMoves;
        //}

        public ulong InterposingMoves(Piece checkingPiece, Piece p)
        {
            // Pawns & knights aren't sliding pieces, their attacks can't be blocked by interposing. The same is true for the king, but the king can't deliver check.
            if (checkingPiece.Type == PieceType.Pawn || checkingPiece.Type == PieceType.Knight)
                return 0;

            // Attackset for checking piece
            ulong checkingPieceMoveset = GenerateMovesForPiece(checkingPiece);

            // Moves for the defending piece
            ulong defendingPieceMoveset = GenerateMovesForPiece(p); // All pseudolegal moves the defending piece can make, that the checking piece can also make
            List<byte> potentialInterposingMoves = new List<byte>(); // List of all positions the defending piece can pseudolegally move to, that might be actual interposing moves.
            ulong defendingPieceInterposingMoves = 0; // Moves that actually interpose the king and the checking piece

            // Squares involved in the moves we are check
            Square oldSquare = p.Square; // The defending pieces current square

            while (defendingPieceMoveset > 0)
            {
                byte moveToAdd = (byte)BitHelper.GetLeastSignificant1Bit(defendingPieceMoveset);
                defendingPieceMoveset ^= (ulong)1 << (byte)BitHelper.GetLeastSignificant1Bit(defendingPieceMoveset);

                Coordinate newSquareCoord = Board.CoordinateValue.FirstOrDefault(square => square.Value == moveToAdd).Key;
                Square newSquare = Board.SquareDict[newSquareCoord];

                if (Board.MoveManager.InterposesOutOfCheck(oldSquare, newSquare))
                {
                    defendingPieceInterposingMoves |= (ulong)1 << moveToAdd;
                }
            }

            // Reset attacksets to the current board
            GetAllAttacks();

            // Moves that can block the sliding pieces direct path to the king
            return checkingPieceMoveset & defendingPieceInterposingMoves;
        }
        #endregion
    }
}

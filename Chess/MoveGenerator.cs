using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class MoveGenerator
    {
        /// <summary>
        /// Generates moves for all pieces on a given board.
        /// </summary>
        /// <param name="board"></param>
        public static void GenerateMoves(Board board)
        {
            foreach (Piece piece in board.Pieces)
            {
                // Only generate moves for the pieces that can be moved on the current turn
                if (piece.Color == board.Turn)
                {
                    GenerateMovesForPiece(piece, board);
                }
            }
        }

        /// <summary>
        /// Generates moves for a specific piece.
        /// </summary>
        /// <param name="piece">The piece to move.</param>
        /// <param name="board">The currently playing board.</param>
        /// <returns>The valid moveset. If the moveset doesn't exist, returns 0.</returns>
        public static ulong GenerateMovesForPiece(Piece piece, Board board)
        {
            // Used to get and remove the moving pieces own pieces from its list of valid moves.
            string ownPieces = "";

            if (board.Turn == Color.White)
                ownPieces = Constants.bbWhitePieces;
            else
                ownPieces = Constants.bbBlackPieces;

            switch (piece.Type)
            {
                
                case PieceType.Pawn:
                
                    switch (piece.Color)
                    {
                        case Color.White:
                            return GetMovesForWhitePawn(piece.Square.Coordinate);

                        case Color.Black:
                            return GetMovesForBlackPawn(piece.Square.Coordinate);

                        default:
                            return 0;
                    }
                
                case PieceType.Knight:
                    return GetMovesForKnight(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.King:
                    return GetMovesForKing(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Rook:
                    return GetMovesForRook(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Bishop:
                    return GetMovesForBishop(piece) & ~BitBoards.BitBoardDict[ownPieces];

                case PieceType.Queen:
                    return (GetMovesForBishop(piece) | GetMovesForRook(piece)) & ~BitBoards.BitBoardDict[ownPieces];

                default:
                    return 0;
            }
        }

        // TODO: En pessant
        // TODO: Promotion
        #region Pawn Move Generation

        public static ulong GetMovesForWhitePawn(Coordinate from)
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

            ulong attackLeft = (startPos & Constants.EmptyFileA) << 7; // The square up and left from the pawn can be attacked, unless the pawn is on File A.
            ulong attackRight = (startPos & Constants.EmptyFileH) << 9; // The square up and right from the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            ulong validAttacks = allAttacks & BitBoards.BitBoardDict["BlackPieces"]; // An attack is valid if it is possible for the pawn to attack that way, and if the attacked square has an opposing piece on it.

            ulong validMoves = validPushes | validAttacks; // Bitboard containing all possible moves for a pawn.

            return validMoves;
        }

        public static ulong GetMovesForBlackPawn(Coordinate from)
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

            ulong attackLeft = (startPos & Constants.EmptyFileA) >> 7; // The square ahead and left of the pawn can be attacked, unless the pawn is on File H.
            ulong attackRight = (startPos & Constants.EmptyFileH) >> 9; // The square ahead and right of the pawn can be attacked, unless the pawn is on File H.
            ulong allAttacks = attackLeft | attackRight; // A combination of all possible attacks.

            ulong validAttacks = allAttacks & BitBoards.BitBoardDict["WhitePieces"]; // An attack is valid if it is possible for the pawn to attack that way, and if the attacked square has an opposing piece on it.

            ulong validMoves = validPushes | validAttacks; // Bitboard containing all possible moves for a pawn.

            return validMoves;
        }

        #endregion

        #region Knight Move Generation

        public static ulong GetMovesForKnight(Piece piece)
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

        public static ulong GetMovesForKing(Piece piece)
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
            ulong spot4 = kingClipFileH << 1;

            ulong spot5 = kingClipFileA >> 1;
            ulong spot6 = startingPos >> 7;
            ulong spot7 = kingClipFileA >> 8;
            ulong spot8 = kingClipFileA >> 9;

            ulong kingValid = spot1 | spot2 | spot3 | spot4 |
                              spot5 | spot6 | spot7 | spot8;

            // Get name of color to pass to bitboard dictionary.
            string color = piece.Color.ToString();
            kingValid = kingValid & ~BitBoards.BitBoardDict[color + "Pieces"];

            return kingValid;

        }

        #endregion

        #region Rook Move Generation

        public static ulong GetMovesForRook(Piece piece)
        {
            // The rooks starting position
            Coordinate from = piece.Square.Coordinate;

            // The value of said square
            byte square = Board.CoordinateValue[from];

            // Rooks can move to all free squares directly north, east, south and west of its starting square.
            ulong rookValid = GetRankMoves(square) | GetFileMoves(square);
            return rookValid;
        }

        #endregion

        #region Bishop Move Generation

        public static ulong GetMovesForBishop(Piece piece)
        {
            // The bishops starting position
            Coordinate from = piece.Square.Coordinate;

            // The value of that square
            byte square = Board.CoordinateValue[from];

            // Generates a bitboard with the pieces starting position.
            ulong startPos = (ulong)1 << Board.CoordinateValue[from];

            // Get all diagonal and antidiagonal moves for whereever the bishop currently is.
            ulong bishopValid = GetDiagonalMoves(square, startPos) | GetAntiDiagonalMoves(square, startPos);

            // DEBUG 
            Trace.WriteLine(BitBoards.BinaryMatrix(bishopValid));
            return bishopValid;
        }

        #endregion

        #region Generate Positive And Negative Moves
        private static ulong GetPositiveMoves(int rayDirection, byte square)
        {
            ulong moves = Ray.Rays[rayDirection][square];
            ulong blocker = moves & BitBoards.BitBoardDict[Constants.bbSquaresOccupied]; // moves on an empty board AND'ed with the current bitboard of all pieces.

            // if this direction is eventually blocked off by a piece.
            if (blocker != 0)
            {
                // using forward bitscan to find the least significant bit
                int blockerSquare = BitHelper.GetLeastSignificant1Bit(blocker);

                moves = moves ^ Ray.Rays[rayDirection][blockerSquare];
            }

            return moves;
        }

        private static ulong GetNegativeMoves(int rayDirection, byte square)
        {
            ulong moves = Ray.Rays[rayDirection][square];
            ulong blocker = moves & BitBoards.BitBoardDict[Constants.bbSquaresOccupied];

            // if this direction is eventually blocked off by a piece.
            if (blocker != 0)
            {
                // using forward bitscan to find the least significant bit
                int blockerSquare = BitHelper.GetMostSignificant1Bit(blocker);

                moves = moves ^ Ray.Rays[rayDirection][blockerSquare];
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
        private static ulong GetRankMoves(byte square)
        {
            ulong westMoves = GetPositiveMoves(Ray.West, square);
            ulong eastMoves = GetNegativeMoves(Ray.East, square);

            ulong moves = eastMoves | westMoves; 
            return moves;
        }

        /// <summary>
        /// Get all squares to the north/south of a given square.
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        private static ulong GetFileMoves(byte square)
        {
            ulong northMoves = GetPositiveMoves(Ray.North, square);
            ulong southMoves = GetNegativeMoves(Ray.South, square);

            ulong moves = northMoves | southMoves;
            return moves;
        }

        #endregion

        #region Generate (anti-)diagonal moves

        private static ulong GetDiagonalMask(byte square)
        {
            ulong diagonal = Constants.Diagonal;
            int diag = (square & 7) - (square >> 3);
            return diag >= 0 ? diagonal >> diag * 8 : diagonal << -diag * 8;
        }
        private static ulong GetDiagonalMoves(byte square, ulong startPos)
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

        private static ulong GetAntiDiagonalMask(byte square)
        {
            ulong antiDiagonal = Constants.AntiDiagonal;
            int diag = 7 - (square & 7) - (square >> 3);
            return diag >= 0 ? antiDiagonal >> diag * 8 : antiDiagonal << -diag * 8;
        }

        private static ulong GetAntiDiagonalMoves(byte square, ulong startPos)
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
        /// <param name="b">The currently playing board.</param>
        /// <param name="from">The starting square.</param>
        /// <param name="to">The proposed end square.</param>
        /// <returns>True if the move can be completed (pseudo-legally), otherwise false.</returns>
        public static bool IsPseudoLegal(Board b, Square from, Square to)
        {
            ulong validMoves = GenerateMovesForPiece(from.Piece, b);

            ulong destinationBB = (ulong)1 << Board.CoordinateValue[to.Coordinate];
            ulong isValidMove = validMoves & destinationBB;

            return (isValidMove > 0);
        }
        
        #endregion
    }
}

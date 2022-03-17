using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Numerics;

namespace Chess
{
    /// <summary>
    /// The bitboards needed to represent the game
    /// </summary>
    internal class BitBoards
    {

        #region BitBoard Variables
        /// <summary>
        /// Bitboard determining the position of the white king.
        /// </summary>
        internal UInt64 WhiteKing;
        /// <summary>
        /// Bitboard determining the position of the white queen(s).
        /// </summary>
        internal UInt64 WhiteQueens;
        /// <summary>
        /// Bitboard determining the position of the white rooks.
        /// </summary>
        internal UInt64 WhiteRooks;
        /// <summary>
        /// Bitboard determining the position of the white bishops.
        /// </summary>
        internal UInt64 WhiteBishops;
        /// <summary>
        /// Bitboard determining the position of the white knights.
        /// </summary>
        internal UInt64 WhiteKnights;
        /// <summary>
        /// Bitboard determining the position of the white pawns.
        /// </summary>
        internal UInt64 WhitePawns;
        /// <summary>
        /// Bitboard determining the position of the white pieces.
        /// </summary>
        internal UInt64 WhitePieces;

        /// <summary>
        /// Bitboard determining the position of the black king.
        /// </summary>
        internal UInt64 BlackKing;
        /// <summary>
        /// Bitboard determining the position of the black queens.
        /// </summary>
        internal UInt64 BlackQueens;
        /// <summary>
        /// Bitboard determining the position of the black rooks.
        /// </summary>
        internal UInt64 BlackRooks;
        /// <summary>
        /// Bitboard determining the position of the black bishops.
        /// </summary>
        internal UInt64 BlackBishops;
        /// <summary>
        /// Bitboard determining the position of the black knights.
        /// </summary>
        internal UInt64 BlackKnights;
        /// <summary>
        /// Bitboard determining the position of the black pawns.
        /// </summary>
        internal UInt64 BlackPawns;
        /// <summary>
        /// Bitboard determining the position of the black pieces.
        /// </summary>
        internal UInt64 BlackPieces;

        /// <summary>
        /// Determines all occupied squares
        /// </summary>
        internal UInt64 SquaresOccupied;
        #endregion

        /// <summary>
        /// A dictionary containing all the bitboards used in the current game.
        /// </summary>
        internal static Dictionary<string, UInt64> BitBoardDict;

        public BitBoards()
        {
            WhiteKing = 8;
            WhiteQueens = 16;
            WhiteRooks = 129;
            WhiteBishops = 36;
            WhiteKnights = 66;
            WhitePawns = 65280;

            BlackKing = 576460752303423488;
            BlackQueens = 1152921504606846976;
            BlackRooks = 9295429630892703744;
            BlackBishops = 2594073385365405696;
            BlackKnights = 4755801206503243776;
            BlackPawns = 71776119061217280;

            WhitePieces = WhiteKing | WhiteQueens |
                WhiteRooks | WhiteBishops |
                WhiteKnights | WhitePawns;
            BlackPieces = BlackKing | BlackQueens |
                BlackRooks | BlackBishops |
                BlackKnights | BlackPawns;

            SquaresOccupied = WhitePieces | BlackPieces;

            BitBoardDict = new Dictionary<string, UInt64>();

            BitBoardDict.Add("WhiteKing", WhiteKing);
            BitBoardDict.Add("WhiteQueens", WhiteQueens);
            BitBoardDict.Add("WhiteRooks", WhiteRooks);
            BitBoardDict.Add("WhiteBishops", WhiteBishops);
            BitBoardDict.Add("WhiteKnights",WhiteKnights);
            BitBoardDict.Add("WhitePawns", WhitePawns);

            BitBoardDict.Add("WhitePieces", WhitePieces);

            BitBoardDict.Add("BlackKing", BlackKing);
            BitBoardDict.Add("BlackQueens", BlackQueens);
            BitBoardDict.Add("BlackRooks", BlackRooks);
            BitBoardDict.Add("BlackBishops", BlackBishops);
            BitBoardDict.Add("BlackKnights", BlackKnights);
            BitBoardDict.Add("BlackPawns", BlackPawns);

            BitBoardDict.Add("BlackPieces", BlackPieces);

            BitBoardDict.Add("SquaresOccupied", SquaresOccupied);
            BitBoardDict.Add("SquaresEmpty", ~SquaresOccupied);
        }

        public static Dictionary<string, UInt64> GenerateBitBoards()
        {
            BitBoards bbs = new BitBoards();
            return BitBoardDict;
        }

        /// <summary>
        /// Returns the specified bitboards binary value as a printable string.
        /// </summary>
        /// <param name="input">The bitboard to turn into a binary string.</param>
        /// <returns>The bitboard in binary.</returns>
        public static string BinaryMatrix(UInt64 input)
        {
            // The base to convert to, i.e. binary
            int to = 2;

            // Converts the input to a (signed) 64 bit decimal integer.
            // Then converts the 64 bit decimal integer to be output as binary.
            // Finally, pads the string with leading 0s to fill out the 8x8 matrix that represents the entire chessboard.
            string binary = Convert.ToString((long)input, to).PadLeft(64, '0');

            // Break the output into an 8x8 matrix.
            binary = Regex.Replace(binary, "(.{" + 8 + "})", "$1" + Environment.NewLine);

            return binary;
        }

        public static string BitBoardAsBinary(UInt64 input)
        {
            // The base to convert to, i.e. binary
            int to = 2;

            // Converts the input to a (signed) 64 bit decimal integer.
            // Then converts the 64 bit decimal integer to be output as binary.
            // Finally, pads the string with leading 0s to fill out the 8x8 matrix that represents the entire chessboard.
            string binary = Convert.ToString((long)input, to).PadLeft(64, '0');

            return binary;
        }

        public static string GetBitBoardByPiece(Piece piece)
        {
            string nameOfBitBoard = piece.Color.ToString() + piece.Type.ToString();

            if (piece.Type != PieceType.King)
            {
                nameOfBitBoard += "s";
            };

            return nameOfBitBoard;
        }

        /// <summary>
        /// Update the bitboard dictionary of a given board
        /// </summary>
        /// <param name="board"></param>
        public void UpdateBitBoards(Board board)
        {
            var dict = BitBoardDict;
            dict["WhitePieces"] = dict["WhiteKing"] | dict["WhiteQueens"] |
                dict["WhiteRooks"] | dict["WhiteBishops"] |
                dict["WhiteKnights"] | dict["WhitePawns"];
            dict["BlackPieces"] = dict["BlackKing"] | dict["BlackQueens"] |
                dict["BlackRooks"] | dict["BlackBishops"] |
                dict["BlackKnights"] | dict["BlackPawns"];
        }

        #region Move making process

        //*************
        //*MAKE A MOVE*
        //*************

        /// <summary>
        /// Moves a piece from 1 square to another and updates relevant bitboards
        /// </summary>
        /// <returns></returns>
        public static bool MakeMove(Board board, double oldX, double oldY, double newX, double newY)
        {
            int oldSquareIndex = 0;
            int newSquareIndex = 0;

            oldY = Math.Floor(oldY / 100) - 1;
            oldX = Math.Floor(oldX / 100) - 1;

            newY = Math.Floor(newY / 100) - 1;
            newX = Math.Floor(newX / 100) - 1;

            oldSquareIndex = ((int)oldX + (8 * (int)oldY));
            newSquareIndex = ((int)newX + (8 * (int)newY));

            Coordinate oldSquareCoords = (Coordinate)oldSquareIndex;
            Coordinate newSquareCoords = (Coordinate)newSquareIndex;

            Square newSquare = board.SquareDict[newSquareCoords];
            Square oldSquare = board.SquareDict[oldSquareCoords];
            Piece piece = oldSquare.Piece;

            if ((newSquare.Piece != null && newSquare.Piece.Color == piece.Color) || piece.Color != board.Turn)
            {
                return false;
            }

            if (!MoveGenerator.IsPseudoLegal(board, oldSquare, newSquare))
            {
                return false;
            }
            else
            {
                ulong fromBoard = BitBoardDict["SquaresOccupied"];

                // Update bitboards

                ulong from = (ulong)1 << (int)Board.CoordinateValue[oldSquareCoords];
                ulong to = (ulong)1 << (int)Board.CoordinateValue[newSquareCoords];
                ulong fromTo = from ^ to;
                BitBoardDict[GetBitBoardByPiece(piece)] ^= fromTo;
                string pieceColor = piece.Color.ToString();
                BitBoardDict[pieceColor + "Pieces"] ^= fromTo;

                // remove opposing piece from the game if any
                if (newSquare.Piece != null)
                {
                    newSquare.Piece.Sprite.Source = null;
                    newSquare.Piece.Square = null;

                    BitBoardDict[GetBitBoardByPiece(newSquare.Piece)] ^= to;

                    string takenPieceColor = newSquare.Piece.Color.ToString();
                    BitBoardDict[takenPieceColor + "Pieces"] ^= to; // 

                    BitBoardDict["SquaresOccupied"] ^= from;
                    BitBoardDict["SquaresEmpty"] ^= from;
                }
                // Apply ^= fromto instead of ^= from, if no capture has taken place.
                else
                {
                    BitBoardDict["SquaresOccupied"] ^= fromTo;
                    BitBoardDict["SquaresEmpty"] ^= fromTo;
                }

                // Move this piece from its previous square
                // to its new square
                oldSquare.Piece = null;
                newSquare.Piece = piece;
                piece.Square = newSquare;


                // DEBUG: Writes all the affected bitboards to output from debug
                //Trace.WriteLine("From: \n" + BitBoardAsBinaryMatrix(from));
                //Trace.WriteLine("To: \n" + BitBoardAsBinaryMatrix(to));
                //Trace.WriteLine("FromTo: \n" + BitBoardAsBinaryMatrix(fromTo));
                //Trace.WriteLine("PieceBB: \n" + BitBoardAsBinaryMatrix(BitBoardDict[GetBitBoardByPiece(piece)]));
                //Trace.WriteLine("ColorBB: \n" + BitBoardAsBinaryMatrix(BitBoardDict[pieceColor + "Pieces"]));
                //Trace.WriteLine("OccSq: \n" + BitBoardAsBinaryMatrix(BitBoardDict["SquaresOccupied"]));
                //Trace.WriteLine("EmpSq: \n" + BitBoardAsBinaryMatrix(BitBoardDict["SquaresEmpty"]));

                board.Turn = (Color)((int)board.Turn * -1);

                return true;
            }
        }

        #endregion
    }
}

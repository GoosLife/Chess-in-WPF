using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess
{
    internal class MoveManager
    {
        public MoveGenerator MoveGenerator { get; set; }
        private Board Board { get; set; }

        public MoveManager(Board board)
        {
            Board = board;
            MoveGenerator = new MoveGenerator(Board);
        }

        #region Move making process

        //*************
        //*MAKE A MOVE*
        //*************

        /// <summary>
        /// Moves a piece from 1 square to another and updates relevant bitboards
        /// </summary>
        /// <returns></returns>
        public bool MakeMove(double oldX, double oldY, double newX, double newY)
        {
            Coordinate newSquareCoords;
            Square newSquare = Board.GetSquare(Board, newX, newY, out newSquareCoords);

            Coordinate oldSquareCoords;
            Square oldSquare = Board.GetSquare(Board, oldX, oldY, out oldSquareCoords);

            Piece piece = oldSquare.Piece;

            if ((newSquare.Piece != null && newSquare.Piece.Color == piece.Color) || piece.Color != Board.Turn)
            {
                return false;
            }

            if (!MoveGenerator.IsPseudoLegal(oldSquare, newSquare))
            {
                return false;
            }
            else
            {
                ulong fromBoard = BitBoards.BitBoardDict["SquaresOccupied"];

                // Update bitboards

                ulong from = (ulong)1 << (int)Board.CoordinateValue[oldSquareCoords];
                ulong to = (ulong)1 << (int)Board.CoordinateValue[newSquareCoords];
                ulong fromTo = from ^ to;
                BitBoards.BitBoardDict[BitBoards.GetBitBoardByPiece(piece)] ^= fromTo;
                string pieceColor = piece.Color.ToString();
                BitBoards.BitBoardDict[pieceColor + "Pieces"] ^= fromTo;

                // remove opposing piece from the game if any
                if (newSquare.Piece != null)
                {
                    newSquare.Piece.Sprite.Source = null;
                    newSquare.Piece.Square = null;

                    BitBoards.BitBoardDict[BitBoards.GetBitBoardByPiece(newSquare.Piece)] ^= to;

                    string takenPieceColor = newSquare.Piece.Color.ToString();
                    BitBoards.BitBoardDict[takenPieceColor + "Pieces"] ^= to; // 

                    BitBoards.BitBoardDict["SquaresOccupied"] ^= from;
                    BitBoards.BitBoardDict["SquaresEmpty"] ^= from;

                    Board.Pieces.Remove(newSquare.Piece);

                    // Remove piece attacks from attack sets // TODO: Implement this better - currently sets value to 0 for all attacksets.
                    MoveGenerator.BlackAttacks[newSquare.Piece] = 0;
                    MoveGenerator.WhiteAttacks[newSquare.Piece] = 0;
                    MoveGenerator.AllAttacks[newSquare.Piece] = 0;
                }
                // Apply ^= fromto instead of ^= from, if no capture has taken place.
                else
                {
                    BitBoards.BitBoardDict["SquaresOccupied"] ^= fromTo;
                    BitBoards.BitBoardDict["SquaresEmpty"] ^= fromTo;
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

                Board.Turn = (Color)((int)Board.Turn * -1);

                MoveGenerator.GetAllAttacks(); // store all attacks from the board as it looks after the latest move.

                return true;
            }
        }

        /// <summary>
        /// Moves a piece from 1 square to another and updates relevant bitboards
        /// </summary>
        /// <returns></returns>
        public bool MakeMove(Square oldSquare, Square newSquare)
        {
            Coordinate newSquareCoords = newSquare.Coordinate;

            Coordinate oldSquareCoords = oldSquare.Coordinate;

            Piece piece = oldSquare.Piece;

            if ((newSquare.Piece != null && newSquare.Piece.Color == piece.Color) || piece.Color != Board.Turn)
            {
                return false;
            }

            if (!MoveGenerator.IsPseudoLegal(oldSquare, newSquare))
            {
                return false;
            }
            else
            {
                ulong fromBoard = BitBoards.BitBoardDict["SquaresOccupied"];

                // Update bitboards

                ulong from = (ulong)1 << (int)Board.CoordinateValue[oldSquareCoords];
                ulong to = (ulong)1 << (int)Board.CoordinateValue[newSquareCoords];
                ulong fromTo = from ^ to;
                BitBoards.BitBoardDict[BitBoards.GetBitBoardByPiece(piece)] ^= fromTo;
                string pieceColor = piece.Color.ToString();
                BitBoards.BitBoardDict[pieceColor + "Pieces"] ^= fromTo;

                // remove opposing piece from the game if any
                if (newSquare.Piece != null)
                {
                    newSquare.Piece.Sprite.Source = null;
                    newSquare.Piece.Square = null;

                    BitBoards.BitBoardDict[BitBoards.GetBitBoardByPiece(newSquare.Piece)] ^= to;

                    string takenPieceColor = newSquare.Piece.Color.ToString();
                    BitBoards.BitBoardDict[takenPieceColor + "Pieces"] ^= to; // 

                    BitBoards.BitBoardDict["SquaresOccupied"] ^= from;
                    BitBoards.BitBoardDict["SquaresEmpty"] ^= from;

                    // Remove piece from list of pieces
                    Board.Pieces.Remove(newSquare.Piece);

                    // Remove piece attacks from attack sets // TODO: Implement this better - currently sets value to 0 for all attacksets.
                    MoveGenerator.BlackAttacks[newSquare.Piece] = 0;
                    MoveGenerator.WhiteAttacks[newSquare.Piece] = 0;
                    MoveGenerator.AllAttacks[newSquare.Piece] = 0;
                }
                // Apply ^= fromto instead of ^= from, if no capture has taken place.
                else
                {
                    BitBoards.BitBoardDict["SquaresOccupied"] ^= fromTo;
                    BitBoards.BitBoardDict["SquaresEmpty"] ^= fromTo;
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

                Board.Turn = (Color)((int)Board.Turn * -1);

                MoveGenerator.GetAllAttacks(); // store all attacks from the board as it looks after the latest move.

                return true;
            }
        }

        /// <summary>
        /// The AI makes one legal move.
        /// </summary>
        public void MakeAIMove()
        {
            foreach (Piece piece in Board.Pieces)
            {
                piece.Moves.Clear();

                if (piece.Color == Board.Turn)
                {
                    // The bitboard containing all legal moves for the piece.
                    ulong allMoves = MoveGenerator.GetMoves(piece);

                    // A list of the bytes representing the individual squares that the piece can move to.
                    List<byte> individualMoves = new List<byte>();

                    // Add each bit from the bitboard to the list of individual, legal moves. Then remove that bit from the bitboard of all moves. Continue until bitboard of all moves is empty.
                    while (allMoves > 0)
                    {
                        byte moveToAdd = (byte)BitHelper.GetLeastSignificant1Bit(allMoves);
                        allMoves ^= (ulong)1 << (byte)BitHelper.GetLeastSignificant1Bit(allMoves);

                        piece.Moves.Add(moveToAdd);
                    }
                }
            }

            Square oldSquare;
            Square newSquare;
            Piece pieceToMove;

            // Try to make moves until a valid move is found.
            do
            {
                Random pieceSelector = new Random();
                List<Piece> eligiblePieces = new List<Piece>(); // Stores only those pieces that have potential moves. I.e., if a piece is shown to have no legal moves, we don't have to check it again.
                
                foreach (var value in Board.Pieces)
                {
                    eligiblePieces.Add(value);
                }

                // Find a piece that has a legal move.
                do
                {
                    pieceToMove = Board.Pieces[pieceSelector.Next(0, eligiblePieces.Count)];
                    Trace.Write($"Moves for {Board.Pieces.Where(p => p == pieceToMove).First().Type} {Board.Pieces.Where(p => p == pieceToMove).First().Color} {Board.Pieces.Where(p => p == pieceToMove).First().Moves.Count}");
                    eligiblePieces.Remove(pieceToMove);
                } while ((!(pieceToMove.Moves.Count > 0)));

                // The square the piece is currently on
                oldSquare = pieceToMove.Square;

                // TODO: Currently, AI just selects random moves.
                Random moveSelector = new Random();

                byte selectedMove = pieceToMove.Moves[moveSelector.Next(0, pieceToMove.Moves.Count)];

                // Determine which square the piece will be on after moving
                Coordinate newSquareCoord = Board.CoordinateValue.FirstOrDefault(c => c.Value == selectedMove).Key;
                newSquare = Board.SquareDict[newSquareCoord];

            } while (!MakeMove(oldSquare, newSquare));

            // Set piece sprite in correct place.
            Canvas.SetTop(pieceToMove.Sprite, (newSquare.Rank * 100) + (Square.Size / 2) - (Piece.SpriteSize / 2));
            Canvas.SetLeft(pieceToMove.Sprite, (newSquare.File * 100) + (Square.Size / 2) - (Piece.SpriteSize / 2));
        }

        #endregion
    }
}

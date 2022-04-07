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

        #region ACTUALLY MAKES MOVES & UPDATE BITBOARDS

        /// <summary>
        /// Move a piece by moving its sprite in the GUI
        /// </summary>
        /// <returns></returns>
        public bool MakeMove(double oldX, double oldY, double newX, double newY)
        {
            Square newSquare = Board.GetSquare(Board, newX, newY);
            Square oldSquare = Board.GetSquare(Board, oldX, oldY);

            return MakeMove(oldSquare, newSquare);
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

            // THIS IS NOW CHECKED BY THE MOVE GENERATOR
            if (piece.Color != Board.Turn)
            {
                return false;
            }

            if (!MoveGenerator.IsPseudoLegal(oldSquare, newSquare))
            {
                return false;
            }
            else
            {
                // Check if move is a castling move
                if (piece.Type == PieceType.King)
                {
                    switch (piece.Color)
                    {
                        case Color.White:
                            // If the move is a short castle, and short castle is currently allowed
                            if (((ulong) 1 << (Board.CoordinateValue[oldSquareCoords]) | ((ulong) 1 << Board.CoordinateValue[newSquareCoords])) == Constants.WhiteCastlingShortMask && Board.EnPassantAndCastling.CanWhiteCastleShort())
                            {
                                MakeProgrammaticMove(Board.SquareDict[Coordinate.H1].Piece, Board.SquareDict[Coordinate.F1]);
                                Board.Turn = (Color)((int)Board.Turn * -1); // Reset turn after making the "extra" rook move. Otherwise, the game thinks black used their turn moving the white rook.
                            }
                            // ... if the move is a long castle, and long castle is currently allowed
                            else if (((ulong) 1 << Board.CoordinateValue[oldSquareCoords] | (ulong) 1 << Board.CoordinateValue[newSquareCoords]) == Constants.WhiteCastlingLongMask && Board.EnPassantAndCastling.CanWhiteCastleLong())
                            {
                                MakeProgrammaticMove(Board.SquareDict[Coordinate.A1].Piece, Board.SquareDict[Coordinate.D1]);
                                Board.Turn = (Color)((int)Board.Turn * -1); // Reset turn after making the "extra" rook move. Otherwise, the game thinks black used their turn moving the white rook.
                            }

                            // King has moved, disable castling rights.
                            Board.EnPassantAndCastling.DisableWhiteShortCastling();
                            Board.EnPassantAndCastling.DisableWhiteLongCastling();

                            break;

                        case Color.Black:
                            // If the move is a short castle, and short castle is currently allowed
                            if (((ulong)1 << (Board.CoordinateValue[oldSquareCoords]) | ((ulong)1 << Board.CoordinateValue[newSquareCoords])) == Constants.BlackCastlingShortMask && Board.EnPassantAndCastling.CanBlackCastleShort())
                            {
                                MakeProgrammaticMove(Board.SquareDict[Coordinate.H8].Piece, Board.SquareDict[Coordinate.F8]);
                                Board.Turn = (Color)((int)Board.Turn * -1); // Reset turn after making the "extra" rook move. Otherwise, the game thinks black used their turn moving the white rook.
                            }
                            else if (((ulong)1 << (Board.CoordinateValue[oldSquareCoords]) | ((ulong)1 << Board.CoordinateValue[newSquareCoords])) == Constants.BlackCastlingLongMask && Board.EnPassantAndCastling.CanBlackCastleLong())
                            {
                                MakeProgrammaticMove(Board.SquareDict[Coordinate.A1].Piece, Board.SquareDict[Coordinate.D1]);
                                Board.Turn = (Color)((int)Board.Turn * -1); // Reset turn after making the "extra" rook move. Otherwise, the game thinks black used their turn moving the white rook.
                            }

                            // King has moved, disable castling rights.
                            Board.EnPassantAndCastling.DisableBlackShortCastling();
                            Board.EnPassantAndCastling.DisableBlackLongCastling();

                            break;
                    }
                }

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
                    if (newSquare.Piece.Color == Color.White)
                    {
                        MoveGenerator.WhiteAttacks[newSquare.Piece] = 0;
                    }
                    else
                    {
                        MoveGenerator.BlackAttacks[newSquare.Piece] = 0;
                    }

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

                Board.Turn = (Color)((int)Board.Turn * -1);

                MoveGenerator.GetAllAttacks(); // store all attacks from the board as it looks after the latest move.

                // TODO: This should probably be in its own class.
                // If a piece is in check, and there are no more legal moves, game ends in checkmate.
                if (MoveGenerator.GetCheckingPiece() != null && IsCheckMate())
                {
                    Trace.Write("Checkmate has occured!"); // TODO: Implement checkmate game over stuff
                }
                else if (IsStaleMate())
                {
                    Trace.WriteLine("Stalemate has occurred!"); // TODO: Implement stalemate game over stuff.
                }

                return true;
            }
        }

        /// <summary>
        /// Moves a piece from 1 square to another and updates relevant bitboards, but pieces can move freely. Useful for setting up boards.
        /// </summary>
        /// <returns></returns>
        public bool MakeFreeMove(double oldX, double oldY, double newX, double newY)
        {
            Coordinate newSquareCoords;
            Square newSquare = Board.GetSquare(Board, newX, newY, out newSquareCoords);

            Coordinate oldSquareCoords;
            Square oldSquare = Board.GetSquare(Board, oldX, oldY, out oldSquareCoords);

            Piece piece = oldSquare.Piece;

            if ((newSquare.Piece != null && newSquare.Piece == piece))
            {
                return false;
            }

            //if (!MoveGenerator.IsPseudoLegal(oldSquare, newSquare))
            //{
            //    return false;
            //}
            //else
            //{
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
                if (newSquare.Piece.Color == Color.White)
                {
                    MoveGenerator.WhiteAttacks[newSquare.Piece] = 0;
                }
                else
                {
                    MoveGenerator.BlackAttacks[newSquare.Piece] = 0;
                }

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

            Board.Turn = (Color)((int)Board.Turn * -1);

            MoveGenerator.GetAllAttacks(); // store all attacks from the board as it looks after the latest move.

            return true;
            // }
        }

        #endregion

        #endregion

        #region CHECK IF MOVES ARE LEGAL DURING CHECK
        /// <summary>
        /// Makes a hypothetical, potentially interposing move. Since we only test moves we know are pseudolegal, this check is skipped, preventing the function from becoming recursive.
        /// Other than that, it is the same as MakeMove(Square oldSquare, Square newSquare)
        /// </summary>
        /// <returns></returns>
        public bool TestInterposingMove(Square oldSquare, Square newSquare)
        {
            Coordinate newSquareCoords = newSquare.Coordinate;

            Coordinate oldSquareCoords = oldSquare.Coordinate;

            Piece piece = oldSquare.Piece;

            ulong fromBoard = BitBoards.BitBoardDict["SquaresOccupied"];

            // Update bitboards

            ulong from = (ulong)1 << (int)Board.CoordinateValue[oldSquareCoords];
            ulong to = (ulong)1 << (int)Board.CoordinateValue[newSquareCoords];
            ulong fromTo = from ^ to;
            BitBoards.BitBoardDict[BitBoards.GetBitBoardByPiece(piece)] ^= fromTo;
            string pieceColor = piece.Color.ToString();
            BitBoards.BitBoardDict[pieceColor + "Pieces"] ^= fromTo;

            BitBoards.BitBoardDict["SquaresOccupied"] ^= fromTo;
            BitBoards.BitBoardDict["SquaresEmpty"] ^= fromTo;

            // Move this piece from its previous square
            // to its new square
            oldSquare.Piece = null;
            newSquare.Piece = piece;
            piece.Square = newSquare;

            Board.Turn = (Color)((int)Board.Turn * -1);

            MoveGenerator.GetAllAttacks(); // store all attacks from the board as it looks after the latest move.

            return true;
        }

        /// <summary>
        /// Determine if an interposing move gets the king out of check or not.
        /// </summary>
        /// <returns></returns>
        public bool InterposesOutOfCheck(Square oldSquare, Square newSquare)
        {
            /// REASON : Consider the following situation:
            /// 
            /// OOXO!
            /// OOOBY
            /// OO!O!
            /// OXOYY
            /// KYYYQ
            /// 
            /// Key: O = Empty square
            ///      X = Bishop moves (pseudolegal)
            ///      Y = Queen moves (pseudolegal)
            ///      ! = Interposing squares
            ///      
            ///      K = King
            ///      Q = Queen
            ///      B = Bishop
            /// 
            /// On 3 occasions, the queen can enter the pseudo-legal path of the bishop, but only one of those gets the king out of check.
            /// We want to find that move and eliminate the others.

            // Holds the current position, so the game can be restored once we're done making hypothetical moves.
            Dictionary<string, ulong> resetDictionary = new Dictionary<string, ulong>();
            Piece resetOldSquarePiece = oldSquare.Piece;
            Piece? resetNewSquarePiece = newSquare.Piece; // Technically, the new square should never have a piece, but leaving this here just in case // TODO: Try removing this and see if problems occur
            Square resetPiecesSquare = oldSquare;
            Color resetTurn = Board.Turn; // Reset the turn on completed check.

            foreach (var entry in BitBoards.BitBoardDict)
                resetDictionary.Add(entry.Key, entry.Value);

            // Make the hypothetical move.
            TestInterposingMove(oldSquare, newSquare);

            if (MoveGenerator.IsPieceStillChecking())
            {
                // Reset dictionary before returning
                foreach (var entry in resetDictionary)
                {
                    // Reset piece & squares // TODO: This could be its own function
                    oldSquare.Piece = resetOldSquarePiece;
                    newSquare.Piece = resetNewSquarePiece;
                    oldSquare.Piece.Square = oldSquare;
                    Board.Turn = resetTurn;
                    BitBoards.BitBoardDict[entry.Key] = entry.Value;
                }

                return false;
            }

            // Reset dictionary before returning
            foreach (var entry in resetDictionary)
            {
                // Reset piece and squares // TODO: This could be its own function
                oldSquare.Piece = resetOldSquarePiece;
                newSquare.Piece = resetNewSquarePiece;
                oldSquare.Piece.Square = oldSquare;
                Board.Turn = resetTurn;
                BitBoards.BitBoardDict[entry.Key] = entry.Value;
            }

            return true;
        }
        #endregion

        #region ALTERNATIVE WAYS TO MAKE MOVES - AI MOVES & PROGRAMMATIC MOVES
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
                    ulong allMoves = piece.Moveset;

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

        // TODO: Make sure piece is never null in MakeProgrammatic move, by disabling castling rights when rook has moved.

        /// <summary>
        /// Make a move without human intervention - i.e. moving the rook when the user has moved the king for castling.
        /// </summary>
        public void MakeProgrammaticMove(Piece piece, Square newSquare)
        {
            MakeMove(piece.Square, newSquare);

            // Set piece sprite in correct place.
            Canvas.SetTop(piece.Sprite, (newSquare.Rank * 100) + (Square.Size / 2) - (Piece.SpriteSize / 2));
            Canvas.SetLeft(piece.Sprite, (newSquare.File * 100) + (Square.Size / 2) - (Piece.SpriteSize / 2));

            MoveGenerator.GetAllAttacks();
        }

        #endregion

        #region 

        // TODO: Move this to its own class probably - a Game Manager or something.
        public bool IsCheckMate()
        {
            // If the color to move next turn has no moves, return check mate.

            foreach (Piece piece in Board.Pieces.Where(p => p.Color == Board.Turn))
            {
                if (MoveGenerator.GetOutOfCheckMoves(piece) > 0)
                    return false;
            }

            return true;
        }

        public bool IsStaleMate()
        {
            // If the color to move next turn has no legal moves, but isn't in check, it's a stale mate.

            foreach (Piece piece in Board.Pieces.Where(p => p.Color == Board.Turn))
            {
                if (MoveGenerator.GenerateMovesForPiece(piece) > 0)
                    return false;
            }

            return true;
        }

        #endregion
    }
}

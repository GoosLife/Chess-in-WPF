using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess
{
    public enum Color
    {
        White,
        Black,
    }

    internal abstract class Piece
    {
        /// <summary>
        /// The spritesize.
        /// Default: 64.
        /// </summary>
        public static int SpriteSize { get; set; } = 64;

        /// <summary>
        /// The image for the pieces sprite.
        /// </summary>
        public Image Sprite { get; set; }

        /// <summary>
        /// The color of the piece.
        /// </summary>
        public Color Color { get; set; }

        public Square? Square { get; set; }

        public abstract Image CreateSprite(double topPos, double leftPos, Color color);

        /// <summary>
        /// DEBUG: Shoddy implementation checking if a move is legal.
        /// This will be done with bitboards asap.
        /// This just checks if a square has a piece of the same color
        /// as the moving piece, or removes a piece of the opposite color.
        /// </summary>
        /// <returns></returns>
        public static bool DebugIsMoveLegal(Board board, double oldX, double oldY, double newX, double newY)
        {
            int oldSquareIndex = 0;
            int newSquareIndex = 0;

            oldY = Math.Floor(oldY / 100) - 1;
            oldX = Math.Floor(oldX / 100) - 1;

            newY = Math.Floor(newY / 100) - 1;
            newX = Math.Floor(newX / 100) - 1;

            oldSquareIndex = ((int)oldX + (8 * (int)oldY));
            newSquareIndex = ((int)newX + (8 * (int)newY));

            Coordinates oldSquareCoords = (Coordinates)oldSquareIndex;
            Coordinates newSquareCoords = (Coordinates)newSquareIndex;

            Square oldSquare = board.SquareDict[oldSquareCoords];
            Piece piece = oldSquare.Piece;

            if (piece == null)
            {
                return false;
            }

            Square newSquare = board.SquareDict[newSquareCoords];

            if (newSquare.Piece != null && newSquare.Piece.Color == piece.Color)
            {
                return false;
            }
            else
            {
                // remove the opposing piece from the game
                if (newSquare.Piece != null)
                {
                    newSquare.Piece.Sprite.Source = null;
                    newSquare.Piece.Square = null;
                }

                // Move this piece from its previous square
                // to its new square
                oldSquare.Piece = null;
                newSquare.Piece = piece;
                piece.Square = newSquare;

                UpdateCoordinates(oldSquareCoords, newSquareCoords, piece);

                return true;
            }
        }

        /// <summary>
        /// Updates a pieces coordinates on the graphical representation of the board.
        /// TODO: Use this to update the bitboards
        /// </summary>
        /// <param name="board">The current playing board.</param>
        /// <param name="oldX">X location of the pieces' sprites old square.</param>
        /// <param name="oldY">The y coordinates of the old square.</param>
        /// <param name="newX">The x coordinates of the neq square.</param>
        /// <param name="newY">The y coordinates of the new square.</param>
        public static void UpdateCoordinates(Coordinates oldSquareCoords, Coordinates newSquareCoords, Piece piece)
        {
            Trace.WriteLine($"Old square was {oldSquareCoords}. Now {piece} is on {newSquareCoords}");
        }
    }
}

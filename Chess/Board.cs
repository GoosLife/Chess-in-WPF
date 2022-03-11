using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess
{
    internal class Board
    {
        public Square[] Squares;

        public Board()
        {
            Squares = new Square[64];

            int squareIterator = 0;

            // Create all squares. Iteration is NOT null-indexed (see NOTE: in Square.cs).
            for (int i = 1; i <= 8; i++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    Square s = new Square(i, j);

                    // TODO: This should probably be in a method for placing the sprite on the board.
                    Canvas.SetTop(s.Sprite, 100 * i);
                    Canvas.SetLeft(s.Sprite, 100 * j);
                    Canvas.SetZIndex(s.Sprite, 0);

                    Squares[squareIterator] = s;
                    squareIterator++;
                }
            }

            Square.SetupPieces(Squares);
        }
    }
}

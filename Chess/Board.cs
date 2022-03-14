using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public enum Coordinates
    {
        A8, B8, C8, D8, E8, F8, G8, H8,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A1, B1, C1, D1, E1, F1, G1, H1,
    }

    internal class Board
    {
        public Dictionary<Coordinates, Square> SquareDict { get; set; }

        /// <summary>
        /// The squares that make up the board.
        /// </summary>
        public Square[] Squares;

        /// <summary>
        /// The bitboards representing the position of the board.
        /// </summary>
        public BitBoards BitBoards { get; set; }

        public Board()
        {
            Squares = new Square[64];
            BitBoards = new BitBoards();

            SquareDict = new Dictionary<Coordinates, Square>();

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

                    SquareDict.Add((Coordinates)squareIterator, s);

                    Squares[squareIterator] = s;
                    squareIterator++;
                }
            }
        }

    }
}

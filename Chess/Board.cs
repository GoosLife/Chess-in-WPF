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
    public enum Coordinate
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
        /// <summary>
        /// Dictionary containing all squares on board.
        /// </summary>
        public Dictionary<Coordinate, Square> SquareDict { get; set; }

        public Dictionary<Coordinate, byte> CoordinateValue = new Dictionary<Coordinate, byte>()
        {
            { Coordinate.A8, 63}, { Coordinate.B8, 62}, { Coordinate.C8, 61}, { Coordinate.D8, 60}, { Coordinate.E8, 59}, { Coordinate.F8, 58}, { Coordinate.G8, 57}, {Coordinate.H8, 56},
            { Coordinate.A7, 55}, { Coordinate.B7, 54}, { Coordinate.C7, 53}, { Coordinate.D7, 52}, { Coordinate.E7, 51}, { Coordinate.F7, 50}, { Coordinate.G7, 49}, {Coordinate.H7, 48},
            { Coordinate.A6, 47}, { Coordinate.B6, 46}, { Coordinate.C6, 45}, { Coordinate.D6, 44}, { Coordinate.E6, 43}, { Coordinate.F6, 42}, { Coordinate.G6, 41}, {Coordinate.H6, 40},
            { Coordinate.A5, 39}, { Coordinate.B5, 38}, { Coordinate.C5, 37}, { Coordinate.D5, 36}, { Coordinate.E5, 35}, { Coordinate.F5, 34}, { Coordinate.G5, 33}, {Coordinate.H5, 32},
            { Coordinate.A4, 31}, { Coordinate.B4, 30}, { Coordinate.C4, 29}, { Coordinate.D4, 28}, { Coordinate.E4, 27}, { Coordinate.F4, 26}, { Coordinate.G4, 25}, {Coordinate.H4, 24},
            { Coordinate.A3, 23}, { Coordinate.B3, 22}, { Coordinate.C3, 21}, { Coordinate.D3, 20}, { Coordinate.E3, 19}, { Coordinate.F3, 18}, { Coordinate.G3, 17}, {Coordinate.H3, 16},
            { Coordinate.A2, 15}, { Coordinate.B2, 14}, { Coordinate.C2, 13}, { Coordinate.D2, 12}, { Coordinate.E2, 11}, { Coordinate.F2, 10}, { Coordinate.G2, 9}, { Coordinate.H2, 8},
            { Coordinate.A1, 7}, { Coordinate.B1, 6}, { Coordinate.C1, 5}, { Coordinate.D1, 4}, { Coordinate.E1, 3}, { Coordinate.F1, 2}, { Coordinate.G1, 1}, { Coordinate.H1, 0}
        };

        /// <summary>
        /// The squares that make up the board.
        /// </summary>
        public Square[] Squares;

        /// <summary>
        /// The bitboards representing the position of the board.
        /// </summary>
        public Dictionary<string, ulong> BitBoardDict { get; set; }

        public Board()
        {
            Squares = new Square[64];
            BitBoardDict = BitBoards.GetBitBoards();

            SquareDict = new Dictionary<Coordinate, Square>();

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

                    SquareDict.Add((Coordinate)squareIterator, s);

                    Squares[squareIterator] = s;
                    squareIterator++;
                }
            }
        }

    }
}

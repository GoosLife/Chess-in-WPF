using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// NOTE: Squares must be iterated from 1-8, because it makes
    /// it easier to multiply the rank/file of the square with 100
    /// to fetch both its location and coordinates.
    /// </summary>
    internal class Square
    {
        /// <summary>
        /// The size of each individual square.
        /// Default: 100.
        /// </summary>
        public static int Size { get; set; }

        /// <summary>
        /// INT from 1-8 representing the rank of the square.
        /// In algebraic notation, this is denoted by a letter (A-H).
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// INT from 1-8 representing the file of the square.
        /// In algebraic notation, this is denoted by a number (1-8).
        /// </summary>
        public int File { get; set; }

        public Coordinate Coordinate { get; set; }

        /// <summary>
        /// Color of the square.
        /// 0 = White
        /// 1 = Black
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// The graphical representation of a square.
        /// </summary>
        public Rectangle Sprite { get; set; }

        public Piece? Piece { get; set; }

        public Square(int rank, int file, Coordinate coordinate)
        {
            Rank = rank;
            File = file;
            Coordinate = coordinate;
            Color = AssignColor();

            Sprite = CreateSprite();

            Size = 100;
        }

        /// <summary>
        /// Automatically assigns color to each square
        /// </summary>
        /// <returns></returns>
        private int AssignColor()
        {
            // If the rank is an even number,
            if (Rank % 2 == 0)
            {
                // and the file is an even number,
                if (File % 2 == 0)
                {
                    // the square is white.
                    return 0;
                }
                // Else it is black.
                return 1;
            }

            // If the rank is uneven,
            else
            {
                // but the file is even,
                if (File % 2 == 0)
                {
                    // the square is black.
                    return 1;
                }
                // Else it is white.
                return 0;
            }
        }

        private Rectangle CreateSprite()
        {
            Rectangle s = new Rectangle();

            if (Color == 0)
            {
                s.Fill = Brushes.White;
            }
            else
            {
                s.Fill = Brushes.Brown;
            }

            s.Width = 100;
            s.Height = 100;

            return s;
        }
    }
}

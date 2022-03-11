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
    /// NOTE: Squares must be iterated from 1-8, because it resembles
    /// the way squares are counted in real-life games of chess.
    ///   Alternatively, A1 would be Rank 0, File 0,
    /// and attempting to move a piece from, say, B1-B5 would be done 
    /// by moving it from File 0 to File 4, which could get confusing.
    /// </summary>
    internal class Square
    {
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

        /// <summary>
        /// Color of the square.
        /// 0 = White
        /// 1 = Black
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// The piece of a square.
        /// 0 = BLANK
        /// 1 = WHITE PAWN
        /// 2 = WHITE BISHOP
        /// 3 = WHITE KNIGHT
        /// 4 = WHITE ROOK
        /// 5 = WHITE QUEEN
        /// 6 = WHITE KING
        /// 
        /// 11 = BLACK PAWN
        /// 12 = BLACK BISHOP
        /// 13 = BLACK KNIGHT
        /// 14 = BLACK ROOK
        /// 15 = BLACK QUEEN
        /// 16 = BLACK KING
        /// </summary>
        public int Piece { get; set; }

        /// <summary>
        /// The graphical representation of a square.
        /// </summary>
        public Rectangle Sprite { get; set; }

        public Square(int rank, int file)
        {
            Rank = rank;
            File = file;
            Color = AssignColor();

            Sprite = CreateSprite();
        }

        /// <summary>
        /// Sets up initial piece positions
        /// </summary>
        /// <returns></returns>
        public static void SetupPieces(Square[] squares)
        {
            List<int> initialPositions = new List<int>() { 4, 3, 2, 5, 6, 2, 3, 4,
                                                           1, 1, 1, 1, 1, 1, 1, 1,
                                                           0, 0, 0, 0, 0, 0, 0, 0,
                                                           0, 0, 0, 0, 0, 0, 0, 0,
                                                           0, 0, 0, 0, 0, 0, 0, 0,
                                                           0, 0, 0, 0, 0, 0, 0, 0,
                                                           11, 11, 11, 11, 11, 11, 11, 11,
                                                           14, 13, 12, 15, 16, 12, 13, 14 };

            for (int i = 0; i < squares.Length; i++)
            {
                squares[i].Piece = initialPositions[i];
                squares[i].Sprite.Tag = initialPositions[i];
            }
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
                s.Fill = Brushes.Black;
            }

            s.Width = 100;
            s.Height = 100;

            return s;
        }
    }
}

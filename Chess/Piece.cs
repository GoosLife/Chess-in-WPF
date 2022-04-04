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
        White = -1,
        Black = 1,
    }

    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
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

        public Square Square { get; set; }

        public PieceType Type { get; set; }

        public abstract Image CreateSprite(double topPos, double leftPos, Color color);

        public List<byte> Moves { get; set; } = new List<byte>();
    }
}

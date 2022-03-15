using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    internal class Pawn : Piece
    {
        public Pawn(double topPos, double leftPos, Color color)
        {
            Type = PieceType.Pawn;
            Sprite = CreateSprite(topPos, leftPos, color);
            Color = color;
        }

        public override Image CreateSprite(double topPos, double leftPos, Color color)
        {
            // Declare the image that will become this pawns sprite.
            Image p = new Image();
            p.Width = 64;
            p.Height = 64;

            // Create bitmap to use as source
            BitmapImage pBitmap = new BitmapImage();

            // UriSource must be set in a BeginInit/EndInit block
            pBitmap.BeginInit();
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Sprites", color.ToString(), this.GetType().Name + ".png");
            pBitmap.UriSource = new Uri(path);

            // Set DecodePixelWidth to save memory, otherwise image will be cached as if it is full-size
            pBitmap.DecodePixelWidth = 64;
            pBitmap.EndInit();

            // Set bitmap as source for image
            p.Source = pBitmap;
            p.Tag = "Pawn";

            // TODO: This should probably be in a method for placing the sprite on the board.
            Canvas.SetTop(p, topPos - p.Height / 2);
            Canvas.SetLeft(p, leftPos - p.Width / 2);
            Canvas.SetZIndex(p, 100);

            return p;
        }
    }
}

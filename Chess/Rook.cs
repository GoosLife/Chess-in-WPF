using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    internal class Rook : Piece
    {
        public Rook(double topPos, double leftPos, Color color)
        {
            Type = PieceType.Rook;
            Sprite = CreateSprite(topPos, leftPos, color);
            Color = color;
        }

        public override Image CreateSprite(double topPos, double leftPos, Color color)
        {
            // Declare the image that will become this pawns sprite.
            Image r = new Image();
            r.Width = 64;
            r.Height = 64;

            // Create bitmap to use as source
            BitmapImage rBitmap = new BitmapImage();

            // UriSource must be set in a BeginInit/EndInit block
            rBitmap.BeginInit();
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Sprites", color.ToString(), this.GetType().Name + ".png");
            rBitmap.UriSource = new Uri(path);

            // Set DecodePixelWidth to save memory, otherwise image will be cached as if it is full-size
            rBitmap.DecodePixelWidth = 64;
            rBitmap.EndInit();

            // Set bitmap as source for image
            r.Source = rBitmap;

            // TODO: This should probably be in a method for placing the sprite on the board.
            Canvas.SetTop(r, topPos - r.Height / 2);
            Canvas.SetLeft(r, leftPos - r.Width / 2);
            Canvas.SetZIndex(r, 100);

            return r;
        }
    }
}

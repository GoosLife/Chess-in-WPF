using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    internal class Queen : Piece
    {
        public Queen(double topPos, double leftPos, Color color)
        {
            Sprite = CreateSprite(topPos, leftPos, color);
            Color = color;
        }

        public override Image CreateSprite(double topPos, double leftPos, Color color)
        {
            // Declare the image that will become this pawns sprite.
            Image sprite = new Image();
            sprite.Width = 64;
            sprite.Height = 64;

            // Create bitmap to use as source
            BitmapImage spriteBitmap = new BitmapImage();

            // UriSource must be set in a BeginInit/EndInit block
            spriteBitmap.BeginInit();
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Sprites", color.ToString(), this.GetType().Name + ".png");
            spriteBitmap.UriSource = new Uri(path);

            // Set DecodePixelWidth to save memory, otherwise image will be cached as if it is full-size
            spriteBitmap.DecodePixelWidth = 64;
            spriteBitmap.EndInit();

            // Set bitmap as source for image
            sprite.Source = spriteBitmap;

            // TODO: This should probably be in a method for placing the sprite on the board.
            Canvas.SetTop(sprite, topPos - sprite.Height / 2);
            Canvas.SetLeft(sprite, leftPos - sprite.Width / 2);
            Canvas.SetZIndex(sprite, 100);

            return sprite;
        }
    }
}

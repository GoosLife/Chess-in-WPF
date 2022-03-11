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
    internal class Pawn
    {
        public Ellipse Sprite { get; set; } // Placeholder sprite for the pawn

        public Pawn()
        {
            Sprite = CreateSprite();
        }

        public static Ellipse CreateSprite()
        {
            Ellipse p = new Ellipse();
            p.Fill = Brushes.Wheat;
            p.Width = 20;
            p.Height = 20;

            return p;
        }
    }
}

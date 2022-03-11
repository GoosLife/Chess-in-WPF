using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Board b;

        public MainWindow()
        {
            InitializeComponent();

            b = new Board();

            foreach (Square s in b.Squares)
            {
                CanvasMain.Children.Add(s.Sprite);
            }

            List<Ellipse> pawns = new List<Ellipse>();

            for (int i = 0; i < 8; i++)
            {
                Pawn pawn = new Pawn();
                pawn.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                pawn.Sprite.MouseDown += Piece_MouseDown;
                
                // Add the physical sprites to the board
                CanvasMain.Children.Add(pawn.Sprite);
            }


            Rectangle rect = new Rectangle();
            rect.Fill = Brushes.LightPink;
            rect.Width = 100;
            rect.Height = 100;
            Canvas.SetTop(rect, 20);
            Canvas.SetLeft(rect, 140);
            Canvas.SetZIndex(rect, -10);
            rect.PreviewMouseDown += Piece_PreviewMouseDown;
            rect.MouseDown += Piece_MouseDown;

            CanvasMain.Children.Add(rect);
        }

        public static UIElement clickObject = null;
        public static UIElement dragObject = null;
        public static Point offset;

        public void Piece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickObject = (UIElement)sender;
            BringToFront(CanvasMain, clickObject);
        }

        static public void BringToFront(Canvas pParent, UIElement pToMove)
        {
            try
            {
                int currentIndex = Canvas.GetZIndex(pToMove);
                int zIndex = 0;
                int maxZ = 0;
                UIElement child;
                for (int i = 0; i < pParent.Children.Count; i++)
                {
                    if (pParent.Children[i] is UIElement &&
                        pParent.Children[i] != pToMove)
                    {
                        child = pParent.Children[i] as UIElement;
                        zIndex = Canvas.GetZIndex(child);
                        maxZ = Math.Max(maxZ, zIndex);
                        if (zIndex > currentIndex)
                        {
                            Canvas.SetZIndex(child, zIndex - 1);
                        }
                    }
                }
                Canvas.SetZIndex(pToMove, maxZ);
            }
            catch (Exception ex)
            {
            }
        }

        public void Piece_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragObject = sender as UIElement;

            BringToFront(CanvasMain, dragObject);

            offset = e.GetPosition(CanvasMain);
            offset.Y -= Canvas.GetTop(dragObject);
            offset.X -= Canvas.GetLeft(dragObject);
            CanvasMain.CaptureMouse();
        }

        private void CanvasMain_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragObject == null)
                return;
            var position = e.GetPosition(sender as IInputElement);
            Canvas.SetTop(dragObject, position.Y - offset.Y);
            Canvas.SetLeft(dragObject, position.X - offset.X);
        }

        private void CanvasMain_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            dragObject = null;
            this.CanvasMain.ReleaseMouseCapture();
        }
    }
}

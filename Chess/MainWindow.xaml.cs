using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            DrawPieces();
        }

        public static UIElement clickObject = null;
        public static UIElement dragObject = null;
        public static Point offset;
        
        // The square that a dragged piece was taken from. Snaps back to this square, when user attempts to place
        // the piece outside of the board.
        Point origSquare;

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

            origSquare.Y = Canvas.GetTop(dragObject);
            origSquare.X = Canvas.GetLeft(dragObject);

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
            Trace.WriteLine(e.ChangedButton == MouseButton.Left);

            bool isCancel = (e.ChangedButton == MouseButton.Right);

            SnapToSquare(sender, e, isCancel);

            dragObject = null;
            this.CanvasMain.ReleaseMouseCapture();
        }

        /// <summary>
        /// Snaps the piece to the closest square.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="isCancel">The user can cancel a move by rightclicking while dragging a piece;
        /// this will cause the isCancel bool to be true.</param>
        private void SnapToSquare(object sender, MouseEventArgs e, bool isCancel)
        {
            int minPos = 100 - (Piece.SpriteSize / 2);
            int maxPos = 800 + (Piece.SpriteSize / 2);

            if (dragObject == null)
                return;
            var position = e.GetPosition(sender as IInputElement);

            // find nearest square
            double topPos = Math.Floor(position.Y / 100d) * 100
                + Square.Size / 2
                - Piece.SpriteSize / 2;
            double leftPos = Math.Floor(position.X / 100d) * 100
                + Square.Size / 2
                - Piece.SpriteSize / 2;

            if ((topPos < minPos || topPos > maxPos) || (leftPos < minPos || leftPos > maxPos) || isCancel)
            {
                topPos = origSquare.Y;
                leftPos = origSquare.X;
            }

            // Check move legality and update logic
            if (!isCancel)
            {
                isCancel = !Piece.DebugIsMoveLegal(b, origSquare.X, origSquare.Y, leftPos, topPos);
            }

            Canvas.SetTop(dragObject, topPos);
            Canvas.SetLeft(dragObject, leftPos);
        }

        public void DrawPieces()
        {
            DrawPawns();
            DrawRooks();
            DrawKnights();
            DrawBishops();
            DrawQueens();
            DrawKings();
        }

        /// <summary>
        /// Draw pawns
        /// </summary>
        /// <param name="input"></param>
        /// <param name="squareIterator"></param>
        public void DrawPawns()
        {
            string whitePawns = BitBoards.BitBoardAsBinary(b.BitBoards.WhitePawns); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whitePawns)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Pawn p = new Pawn(topPos, leftPos, Color.White);
                    p.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    p.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(p.Sprite);

                    // Give piece to square
                    p.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = p;
                }
                // Check next square
                squareIterator++;
            }

            string blackPawns = BitBoards.BitBoardAsBinary(b.BitBoards.BlackPawns);
            squareIterator = 0;

            foreach (char c in blackPawns)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Pawn p = new Pawn(topPos, leftPos, Color.Black);
                    p.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    p.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(p.Sprite);

                    // Give piece to square
                    p.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = p;
                }
                // Check next square
                squareIterator++;
            }
        }

        public void DrawRooks()
        {
            string whiteRooks = BitBoards.BitBoardAsBinary(b.BitBoards.WhiteRooks); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whiteRooks)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Rook r = new Rook(topPos, leftPos, Color.White);
                    r.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    r.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(r.Sprite);

                    // Give piece to square
                    r.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = r;
                }
                // Check next square
                squareIterator++;
            }

            string blackRooks = BitBoards.BitBoardAsBinary(b.BitBoards.BlackRooks);
            Trace.Write(BitBoards.BitBoardAsBinaryMatrix(b.BitBoards.BlackRooks));
            squareIterator = 0;

            foreach (char c in blackRooks)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Rook r = new Rook(topPos, leftPos, Color.Black);
                    r.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    r.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(r.Sprite);

                    // Give piece to square
                    r.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = r;
                }
                // Check next square
                squareIterator++;
            }
        }

        public void DrawKnights()
        {
            string whiteKnights = BitBoards.BitBoardAsBinary(b.BitBoards.WhiteKnights); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whiteKnights)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Knight k = new Knight(topPos, leftPos, Color.White);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }

            string blackKnights = BitBoards.BitBoardAsBinary(b.BitBoards.BlackKnights);
            squareIterator = 0;

            foreach (char c in blackKnights)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Knight k = new Knight(topPos, leftPos, Color.Black);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }
        }

        public void DrawBishops()
        {
            string whiteBishops = BitBoards.BitBoardAsBinary(b.BitBoards.WhiteBishops); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whiteBishops)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Bishop b = new Bishop(topPos, leftPos, Color.White);
                    b.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    b.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(b.Sprite);

                    // Give piece to square
                    b.Square = this.b.SquareDict[(Coordinates)squareIterator];
                    this.b.Squares[squareIterator].Piece = b;
                }
                // Check next square
                squareIterator++;
            }

            string blackBishops = BitBoards.BitBoardAsBinary(b.BitBoards.BlackBishops);
            squareIterator = 0;

            foreach (char c in blackBishops)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Bishop b = new Bishop(topPos, leftPos, Color.Black);
                    b.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    b.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(b.Sprite);

                    // Give piece to square
                    b.Square = this.b.SquareDict[(Coordinates)squareIterator];
                    this.b.Squares[squareIterator].Piece = b;
                }
                // Check next square
                squareIterator++;
            }
        }

        public void DrawQueens()
        {
            string whiteQueens = BitBoards.BitBoardAsBinary(b.BitBoards.WhiteQueens); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whiteQueens)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Queen q = new Queen(topPos, leftPos, Color.White);
                    q.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    q.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(q.Sprite);

                    // Give piece to square
                    q.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = q;
                }
                // Check next square
                squareIterator++;
            }

            string blackQueens = BitBoards.BitBoardAsBinary(b.BitBoards.BlackQueens);
            squareIterator = 0;

            foreach (char c in blackQueens)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Queen q = new Queen(topPos, leftPos, Color.Black);
                    q.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    q.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(q.Sprite);

                    // Give piece to square
                    q.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = q;
                }
                // Check next square
                squareIterator++;
            }
        }

        public void DrawKings()
        {
            string whiteKing = BitBoards.BitBoardAsBinary(b.BitBoards.WhiteKing); // DEBUG: CHANGE TO PAWNS
            int squareIterator = 0;

            foreach (char c in whiteKing)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    King k = new King(topPos, leftPos, Color.White);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }

            string blackKing = BitBoards.BitBoardAsBinary(b.BitBoards.BlackKing);
            squareIterator = 0;

            foreach (char c in blackKing)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.b.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    King k = new King(topPos, leftPos, Color.Black);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = b.SquareDict[(Coordinates)squareIterator];
                    b.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }
        }
    }
}

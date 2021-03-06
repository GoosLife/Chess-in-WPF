using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        Board board;
        bool moveFreely = false; // DEBUG
        Label lblTurn = new Label();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            board = new Board(); // Instantialize board
            BitBoards.GenerateBitBoards(); // Generate new bitboards

            // Draw board squares
            foreach (Square s in board.Squares)
            {
                CanvasMain.Children.Add(s.Sprite);
            }

            DrawPieces();

            board.GetPieces(); // Board retrieves a list of its own pieces.

            Ray.GenerateRays();

            board.MoveManager.MoveGenerator.GetAllAttacks();

            #region DEBUG
            // DEBUG
            lblTurn.Content = board.Turn.ToString();
            Canvas.SetLeft(lblTurn, 1550);
            Canvas.SetTop(lblTurn, 150);
            lblTurn.Name = "lblTurn";

            CanvasMain.Children.Add(lblTurn);
            // END DEBUG
            #endregion
        }


        public static UIElement clickObject = null;
        public static UIElement dragObject = null;
        public static Point offset;

        bool isCancel;
        
        // The square that a dragged piece was taken from. Snaps back to this square, when user attempts to place
        // the piece illegally.
        Point origSquare;

        public void Piece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickObject = (UIElement)sender;
        }

        public void Piece_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragObject = sender as UIElement;

            Canvas.SetZIndex(dragObject, 1000); // Make sure dragged piece is always in front.

            origSquare.Y = Canvas.GetTop(dragObject);
            origSquare.X = Canvas.GetLeft(dragObject);

            // TODO:
            // Calculate moves and show squares that can be moved to
            // moveGenerator.SomeFunctionShowSquares(origSquare.Y, origSquare.X); // TODO: Maybe have a function that gets the square from the coordinates

            HighlightValidSquares();

            offset = e.GetPosition(CanvasMain);
            offset.Y -= Canvas.GetTop(dragObject);
            offset.X -= Canvas.GetLeft(dragObject);
            CanvasMain.CaptureMouse();
        }

        private void CanvasMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Immediately cancel the move if the user presses the right button
            if (e.ChangedButton == MouseButton.Right)
            {
                
                if (dragObject != null)
                {
                    Canvas.SetZIndex(dragObject, 100); // Reset dragged pieces' Z-index as it is placed back on the board.
                }

                isCancel = true;
                SnapToSquare(sender, e, true);
                dragObject = null;
                this.CanvasMain.ReleaseMouseCapture();
            }
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
            if (!isCancel)
            {
                isCancel = (e.ChangedButton == MouseButton.Right);

                SnapToSquare(sender, e, isCancel);

                if (dragObject != null)
                {
                    Canvas.SetZIndex(dragObject, 100); // Reset dragged pieces' Z-index as it is placed back on the board.
                }

                dragObject = null;
                this.CanvasMain.ReleaseMouseCapture();
            }
            isCancel = false;

            RemoveSquareHighlights(); // Remove the highlight dots from the squares.

            lblTurn.Content = board.Turn.ToString(); // DEBUG: Set label to display current turn
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
            var position = e.GetPosition(sender as IInputElement) - offset;

            // find nearest square
            double topPos = Math.Round(position.Y / 100d) * 100
                + Square.Size / 2
                - Piece.SpriteSize / 2;
            double leftPos = Math.Round((position.X - 32) / 100d) * 100
                + Square.Size / 2
                - Piece.SpriteSize / 2;

            var newSquareIndex = ((int)(leftPos / 100 - 1) + (8 * (int)(topPos / 100 - 1)));

            // Cancel move if it is out of bounds
            if ((topPos < minPos || topPos > maxPos) || (leftPos < minPos || leftPos > maxPos))
            {
                isCancel = true;
            }

            // Check move legality and update logic
            if (!isCancel)
            {
                if (!moveFreely)
                    // Move has another chance to cancel if MakeMove returns false
                    isCancel = !board.MoveManager.MakeMove(origSquare.X, origSquare.Y, leftPos, topPos);
                else
                    // Same check, but this time piece can move anywhere // DEBUG
                    isCancel = !board.MoveManager.MakeFreeMove(origSquare.X, origSquare.Y, leftPos, topPos);
            }

            if (isCancel) // reset coordinates as move can't be completed
            {
                topPos = origSquare.Y;
                leftPos = origSquare.X;
            }

            Canvas.SetTop(dragObject, topPos);
            Canvas.SetLeft(dragObject, leftPos);
        }

        #region Highlight valid squares for move

        /// <summary>
        /// Highlights the squares the currently selected piece can move to.
        /// </summary>
        private void HighlightValidSquares()
        {
            Square s = board.GetSquare(board, origSquare.X, origSquare.Y);
            Piece p = s.Piece;

            int dotSize = 10;

            if (p.Color == board.Turn)
            {
                string validMoves = BitBoards.BitBoardAsBinary(board.MoveManager.MoveGenerator.GetMoves(p));
                int squareIterator = 0;

                foreach (char c in validMoves)
                {
                    if (c == '1')
                    {
                        // Get position of piece's square
                        double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                            + Square.Size / 2
                            - dotSize / 2;
                        double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                            + Square.Size / 2
                            - dotSize / 2;

                        // Create new pawn
                        Ellipse dot = new Ellipse();
                        dot.Height = 10;
                        dot.Width = 10;
                        dot.Stroke = Brushes.Black;
                        dot.Fill = new SolidColorBrush(Colors.Gold);
                        dot.Tag = "dot";
                        Canvas.SetTop(dot, topPos);
                        Canvas.SetLeft(dot, leftPos);
                        Canvas.SetZIndex(dot, 999);

                        CanvasMain.Children.Add(dot);
                    }

                    squareIterator++;
                }
            }
        }

        private void RemoveSquareHighlights()
        {
            for (int i = CanvasMain.Children.Count - 1; i >= 0; i--)
            {
                UIElement Child = CanvasMain.Children[i];
                if (Child is Ellipse)
                    CanvasMain.Children.Remove(Child);
            }
        }

        #endregion

        #region Drawing Pieces
        public void DrawPieces()
        {
            DrawPawns();
            DrawRooks();
            DrawKnights();
            DrawBishops();
            DrawQueens();
            DrawKings();
        }
        public void DrawPawns()
        {
            string whitePawns = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhitePawns"]);
            int squareIterator = 0;

            foreach (char c in whitePawns)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Pawn p = new Pawn(topPos, leftPos, Color.White);
                    p.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    p.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(p.Sprite);

                    // Give piece to square
                    p.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = p;
                }
                // Check next square
                squareIterator++;
            }

            string blackPawns = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackPawns"]);
            squareIterator = 0;

            foreach (char c in blackPawns)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Pawn p = new Pawn(topPos, leftPos, Color.Black);
                    p.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    p.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(p.Sprite);

                    // Give piece to square
                    p.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = p;
                }
                // Check next square
                squareIterator++;
            }
        }
        public void DrawRooks()
        {
            string whiteRooks = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhiteRooks"]);
            int squareIterator = 0;

            foreach (char c in whiteRooks)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Rook r = new Rook(topPos, leftPos, Color.White);
                    r.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    r.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(r.Sprite);

                    // Give piece to square
                    r.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = r;
                }
                // Check next square
                squareIterator++;
            }

            string blackRooks = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackRooks"]);
            squareIterator = 0;

            foreach (char c in blackRooks)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Rook r = new Rook(topPos, leftPos, Color.Black);
                    r.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    r.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(r.Sprite);

                    // Give piece to square
                    r.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = r;
                }
                // Check next square
                squareIterator++;
            }
        }
        public void DrawKnights()
        {
            string whiteKnights = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhiteKnights"]);
            int squareIterator = 0;

            foreach (char c in whiteKnights)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Knight k = new Knight(topPos, leftPos, Color.White);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }

            string blackKnights = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackKnights"]);
            squareIterator = 0;

            foreach (char c in blackKnights)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Knight k = new Knight(topPos, leftPos, Color.Black);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }
        }
        public void DrawBishops()
        {
            string whiteBishops = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhiteBishops"]);
            int squareIterator = 0;

            foreach (char c in whiteBishops)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Bishop b = new Bishop(topPos, leftPos, Color.White);
                    b.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    b.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(b.Sprite);

                    // Give piece to square
                    b.Square = this.board.SquareDict[(Coordinate)squareIterator];
                    this.board.Squares[squareIterator].Piece = b;
                }
                // Check next square
                squareIterator++;
            }

            string blackBishops = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackBishops"]);
            squareIterator = 0;

            foreach (char c in blackBishops)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Bishop b = new Bishop(topPos, leftPos, Color.Black);
                    b.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    b.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(b.Sprite);

                    // Give piece to square
                    b.Square = this.board.SquareDict[(Coordinate)squareIterator];
                    this.board.Squares[squareIterator].Piece = b;
                }
                // Check next square
                squareIterator++;
            }
        }
        public void DrawQueens()
        {
            string whiteQueens = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhiteQueens"]);
            int squareIterator = 0;

            foreach (char c in whiteQueens)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Queen q = new Queen(topPos, leftPos, Color.White);
                    q.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    q.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(q.Sprite);

                    // Give piece to square
                    q.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = q;
                }
                // Check next square
                squareIterator++;
            }

            string blackQueens = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackQueens"]);
            squareIterator = 0;

            foreach (char c in blackQueens)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    Queen q = new Queen(topPos, leftPos, Color.Black);
                    q.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    q.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(q.Sprite);

                    // Give piece to square
                    q.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = q;
                }
                // Check next square
                squareIterator++;
            }
        }
        public void DrawKings()
        {
            string whiteKing = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["WhiteKing"]);
            int squareIterator = 0;

            foreach (char c in whiteKing)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    King k = new King(topPos, leftPos, Color.White);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }

            string blackKing = BitBoards.BitBoardAsBinary(BitBoards.BitBoardDict["BlackKing"]);
            squareIterator = 0;

            foreach (char c in blackKing)
            {
                if (c == '1')
                {
                    // Get position of piece's square
                    double topPos = Canvas.GetTop(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;
                    double leftPos = Canvas.GetLeft(this.board.Squares[squareIterator].Sprite)
                        + Square.Size / 2;

                    // Create new pawn
                    King k = new King(topPos, leftPos, Color.Black);
                    k.Sprite.PreviewMouseDown += Piece_PreviewMouseDown;
                    k.Sprite.MouseDown += Piece_MouseDown;

                    // Add the physical sprites to the board
                    CanvasMain.Children.Add(k.Sprite);

                    // Give piece to square
                    k.Square = board.SquareDict[(Coordinate)squareIterator];
                    board.Squares[squareIterator].Piece = k;
                }
                // Check next square
                squareIterator++;
            }
        }

        #endregion

        /// <summary>
        /// DEBUG: Force the computer to make one move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            board.MoveManager.MakeAIMove();

            // DEBUG: Set label to return current move
            lblTurn.Content = board.Turn.ToString();
        }

        private void FreeMoveToggle_Click(object sender, RoutedEventArgs e)
        {
            moveFreely = !moveFreely;

            if (moveFreely)
                FreeMoveToggle.Content = "Moving freely";
            else
                FreeMoveToggle.Content = "Following rules";
        }
    }
}

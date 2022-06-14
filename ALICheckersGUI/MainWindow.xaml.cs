using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using ALICheckersLogic;
using Color = ALICheckersLogic.Color;

namespace ALICheckersGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Board board = new Board(8);
        DispatcherTimer dt = new DispatcherTimer();

        readonly Dictionary<Piece, BitmapImage> images = new Dictionary<Piece, BitmapImage>
        {
            { Piece.Blocked, new BitmapImage(new Uri("pack://application:,,,/Images/bg1.png"))},
            { Piece.Empty,  new BitmapImage(new Uri("pack://application:,,,/Images/bg2.png"))},
            { Piece.WhitePawn, new BitmapImage(new Uri("pack://application:,,,/Images/p1_pawn.png")) },
            { Piece.BlackPawn, new BitmapImage(new Uri("pack://application:,,,/Images/p2_pawn.png")) },
            { Piece.WhiteKing, new BitmapImage(new Uri("pack://application:,,,/Images/p1_king.png")) },
            { Piece.BlackKing, new BitmapImage(new Uri("pack://application:,,,/Images/p2_king.png")) }
        };

        Dictionary<Color, bool> isCPU = new Dictionary<Color, bool>();
        int refreshrate = 50;

        const int SQUARE_SIZE = 50;

        static readonly (int y, int x) FROM_EMPTY = (-1, -1);
        (int y, int x) from = FROM_EMPTY;

        Semaphore semaphore = new Semaphore(0, 1);

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BoardCanvas_Draw(BoardCanvas);
            StatusLabel.Content = "Game started.";

            var configDialog = new ConfigWindow();
            if(configDialog.ShowDialog() == true) {
                isCPU[Color.Black] = configDialog.Player1CPU;
                isCPU[Color.White] = configDialog.Player2CPU;
                refreshrate = configDialog.RefreshRate;
            }
            else
            {
                this.Close();
            }

            if (isCPU[Color.Black] || isCPU[Color.White])
            {
                Task.Run(async () =>
                {
                    while (!board.IsFinished())
                    {
                        if (isCPU[board.playing])
                        {
                            (int score, Board newBoard) = board.Minmax(8);
                            if (newBoard != null)
                                board = newBoard;
                            else
                                MessageBox.Show("Minmax could not calculate next state.");
                        }
                        else
                        {
                            semaphore.WaitOne();
                        }
                    }
                });
            }

            dt.Tick += new EventHandler((object sender, EventArgs e) => {
                BoardCanvas_Draw(BoardCanvas);
            });
            dt.Interval = new TimeSpan(0, 0, 0, 0, refreshrate);
            dt.Start();
        }

        private void BoardCanvas_Draw(Canvas c)
        {
            c.Children.Clear();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i, j) == from)
                    {
                        Rectangle rect = new Rectangle { Width = SQUARE_SIZE, Height = SQUARE_SIZE, Fill = Brushes.LightGray, Opacity = 0.5 };
                        Canvas.SetLeft(rect, from.x * SQUARE_SIZE);
                        Canvas.SetTop(rect, from.y * SQUARE_SIZE);
                        BoardCanvas.Children.Add(rect);
                    }
                    else
                    {
                        BitmapImage src = (j + i) % 2 == 0 ? images[Piece.Blocked] : images[Piece.Empty];
                        Image imgBg = new Image { Width = src.Width, Height = src.Height, Source = src };
                        Canvas.SetLeft(imgBg, SQUARE_SIZE * j);
                        Canvas.SetTop(imgBg, SQUARE_SIZE * i);
                        c.Children.Add(imgBg);
                    }

                    if (board[i, j].IsPiece())
                    {
                        BitmapImage src = images[board[i, j]];
                        Image imgPiece = new Image { Width = src.Width, Height = src.Height, Source = src };
                        Canvas.SetLeft(imgPiece, SQUARE_SIZE * j);
                        Canvas.SetTop(imgPiece, SQUARE_SIZE * i);
                        c.Children.Add(imgPiece);
                    }
                }
            }
        }

        private void BoardCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isCPU[board.playing])
                return;

            int x = (int)e.GetPosition(BoardCanvas).X / SQUARE_SIZE;
            int y = (int)e.GetPosition(BoardCanvas).Y / SQUARE_SIZE;

            if (from == FROM_EMPTY)
            {
                if (board[y, x].IsPiece() && board[y, x].GetColor() == board.playing)
                {
                    from = (y, x);
                }
            }
            else
            {
                (int y, int x) to = (y, x);
                Board newBoard = board.NextState(from, to, true);
                if (newBoard != null)
                {
                    board = newBoard;
                    semaphore.Release();
                }
                else
                {
                    StatusLabel.Content = $"Invalid Move: {from} to {to}";
                }
                from = FROM_EMPTY;
            }
            BoardCanvas_Draw(BoardCanvas);
        }
    }
}

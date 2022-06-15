﻿using System;
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

        readonly Dictionary<Piece, BitmapImage> images = new Dictionary<Piece, BitmapImage>
        {
            { Piece.Blocked, new BitmapImage(new Uri("pack://application:,,,/Images/bg1.png"))},
            { Piece.Empty,  new BitmapImage(new Uri("pack://application:,,,/Images/bg2.png"))},
            { Piece.WhitePawn, new BitmapImage(new Uri("pack://application:,,,/Images/p1_pawn.png")) },
            { Piece.BlackPawn, new BitmapImage(new Uri("pack://application:,,,/Images/p2_pawn.png")) },
            { Piece.WhiteKing, new BitmapImage(new Uri("pack://application:,,,/Images/p1_king.png")) },
            { Piece.BlackKing, new BitmapImage(new Uri("pack://application:,,,/Images/p2_king.png")) }
        };

        Dictionary<Color, bool> isCPU = new Dictionary<Color, bool>
        {
            { Color.Black, false },
            { Color.White, false }
        };
        int refreshrate = 50;

        const int SQUARE_SIZE = 50;

        static readonly (int y, int x) FROM_EMPTY = (-1, -1);
        (int y, int x) from = FROM_EMPTY;

        bool pausedAI = false;

        DispatcherTimer dt = new DispatcherTimer();
        SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGUI();

            ShowOptions();

            Task.Run(async () =>
            {
                while (true)
                {
                    if (!pausedAI && isCPU[board.playing] && !board.IsFinished())
                    {
                        (int score, Board newBoard) = board.Minmax(8);
                        if (newBoard != null)
                        {
                            // Abort if paused mid minmax call.
                            if (!pausedAI)
                                SetNewBoard(newBoard);
                        } 
                        else MessageBox.Show("Minmax could not calculate next state.");
                    }
                    else
                    {
                        semaphore.Wait();
                    }
                }
            });

            dt.Tick += new EventHandler((object sender, EventArgs e) => {
                UpdateGUI();
            });
            dt.Interval = new TimeSpan(0, 0, 0, 0, refreshrate);
            dt.Start();
        }

        #region Utils
        private void ShowOptions()
        {
            var configDialog = new ConfigWindow(isCPU[Color.Black], isCPU[Color.White], refreshrate);
            configDialog.ShowDialog();
            isCPU[Color.Black] = configDialog.Player1CPU;
            isCPU[Color.White] = configDialog.Player2CPU;
            refreshrate = configDialog.RefreshRate;
        }

        private void SetNewBoard(Board newBoard)
        {
            board = newBoard;
            
        }

        private void SetPauseAI(bool newPausedAI)
        {
            pausedAI = newPausedAI;

            if (newPausedAI == true)
            {
                PauseAIButton.Content = "Unpause AI";
            }
            else
            {
                PauseAIButton.Content = "Pause AI";
                UnblockCPU();
            }
        }

        private void UnblockCPU()
        {
            if (isCPU[board.playing] && semaphore.CurrentCount == 0)
                semaphore.Release();
        }
        #endregion

        #region GUI
        private void UpdateGUI()
        {
            BoardCanvas_Draw(BoardCanvas);
            PlayerLabel.Content = board.playing == Color.Black ? "Red" : "Blue";
            ScoreLabel.Content = (board.GetScore() / 10).ToString();
        }

        private void BoardCanvas_Draw(Canvas c)
        {
            c.Children.Clear();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    BitmapImage srcBg = (j + i) % 2 == 0 ? images[Piece.Blocked] : images[Piece.Empty];
                    Image imgBg = new Image { Width = srcBg.Width, Height = srcBg.Height, Source = srcBg };
                    Canvas.SetLeft(imgBg, SQUARE_SIZE * j);
                    Canvas.SetTop(imgBg, SQUARE_SIZE * i);
                    c.Children.Add(imgBg);

                    if (board[i, j].IsPiece())
                    {
                        BitmapImage srcPiece = images[board[i, j]];
                        Image imgPiece = new Image { Width = srcPiece.Width, Height = srcPiece.Height, Source = srcPiece };
                        Canvas.SetLeft(imgPiece, SQUARE_SIZE * j);
                        Canvas.SetTop(imgPiece, SQUARE_SIZE * i);
                        c.Children.Add(imgPiece);
                    }
                }
            }

            if (from != FROM_EMPTY)
            {
                Rectangle rect = new Rectangle { Width = SQUARE_SIZE, Height = SQUARE_SIZE, Fill = Brushes.LightGray, Opacity = 0.5 };
                Canvas.SetLeft(rect, from.x * SQUARE_SIZE);
                Canvas.SetTop(rect, from.y * SQUARE_SIZE);
                BoardCanvas.Children.Add(rect);
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
                    UnblockCPU();
                }
                else
                {
                    StatusLabel.Content = $"Invalid Move: {from} to {to}";
                }
                from = FROM_EMPTY;
            }
            UpdateGUI();
        }

        private void PauseAIButton_Click(object sender, RoutedEventArgs e)
        {
            SetPauseAI(!pausedAI);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            SetPauseAI(true);
            ShowOptions();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            board = new Board(8);
            SetPauseAI(true);
        }
        #endregion
    }
}

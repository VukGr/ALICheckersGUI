using ALICheckersLogic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ALICheckersGUI
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow(bool p1CPU, bool p2CPU, int cpu1Depth, int cpu2Depth, (int piece, int position, int random) cpu1Weights, (int piece, int position, int random) cpu2Weights, int refreshrate)
        {
            InitializeComponent();
            P1CpuCheckBox.IsChecked = p1CPU;
            P2CpuCheckBox.IsChecked = p2CPU;

            CPU1DepthInput.Text = cpu1Depth.ToString();
            CPU2DepthInput.Text = cpu2Depth.ToString();

            CPU1PiecesInput.Text = cpu1Weights.piece.ToString();
            CPU1PositionInput.Text = cpu1Weights.position.ToString();
            CPU1RandomInput.Text = cpu1Weights.random.ToString();

            CPU2PiecesInput.Text = cpu2Weights.piece.ToString();
            CPU2PositionInput.Text = cpu2Weights.position.ToString();
            CPU2RandomInput.Text = cpu2Weights.random.ToString();

            RefreshRateInput.Text = refreshrate.ToString();
        }

        public bool Player1CPU 
        {
            get { return P1CpuCheckBox.IsChecked?? false; }
        }
        public bool Player2CPU
        {
            get { return P2CpuCheckBox.IsChecked?? false; }
        }

        public int RefreshRate
        {
            get {
                int res;
                return Int32.TryParse(RefreshRateInput.Text, out res) ? res : 50; 
            }
        }

        public bool UseCache
        {
            get { return UseCacheCheckBox.IsChecked?? false; }
        }

        public int CPU1Depth
        {
            get
            {
                int res;
                return Int32.TryParse(CPU1DepthInput.Text, out res) ? res : 6;
            }
        }

        public int CPU2Depth
        {
            get
            {
                int res;
                return Int32.TryParse(CPU2DepthInput.Text, out res) ? res : 6;
            }
        }

        public (int Piece, int Position, int Random) CPU1Weights
        {
            get
            {
                int pc = Int32.TryParse(CPU1PiecesInput.Text, out pc) ? pc : 1000;
                int pos = Int32.TryParse(CPU1PositionInput.Text, out pos) ? pos : 1000;
                int rng = Int32.TryParse(CPU1RandomInput.Text, out rng) ? rng : 1000;
                return (pc, pos, rng);
            }
        }

        public (int Piece, int Position, int Random) CPU2Weights
        {
            get
            {
                int pc = Int32.TryParse(CPU2PiecesInput.Text, out pc) ? pc : 1000;
                int pos = Int32.TryParse(CPU2PositionInput.Text, out pos) ? pos : 1000;
                int rng = Int32.TryParse(CPU2RandomInput.Text, out rng) ? rng : 1000;
                return (pc, pos, rng);
            }
        }

        private void DoneButtons_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LoadCacheButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Board.LoadCache(openFileDialog.FileName);
        }

        private void SaveCacheButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
                Board.SaveCache(saveFileDialog.FileName);
        }

        private void P1CpuCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isEnabled = P1CpuCheckBox.IsChecked ?? false;
            CPU1DepthInput.IsEnabled = isEnabled;
            CPU1PiecesInput.IsEnabled = isEnabled;
            CPU1PositionInput.IsEnabled = isEnabled;
            CPU1RandomInput.IsEnabled = isEnabled;
        }

        private void P2CpuCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isEnabled = P2CpuCheckBox.IsChecked ?? false;
            CPU2DepthInput.IsEnabled = isEnabled;
            CPU2PiecesInput.IsEnabled = isEnabled;
            CPU2PositionInput.IsEnabled = isEnabled;
            CPU2RandomInput.IsEnabled = isEnabled;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            P1CpuCheckBox_Checked(sender, e);
            P2CpuCheckBox_Checked(sender, e);
        }
    }
}

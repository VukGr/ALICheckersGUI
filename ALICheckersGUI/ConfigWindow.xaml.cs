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
        public ConfigWindow(bool p1CPU, bool p2CPU, int refreshrate)
        {
            InitializeComponent();
            P1CpuCheckBox.IsChecked = p1CPU;
            P2CpuCheckBox.IsChecked = p2CPU;
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
    }
}

using System;
using System.Windows;
using Microsoft.Win32;
using Zero2UndubProcess;

namespace Zero2Undub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string JpIsoFile { get; set; }
        private string UsIsoFile { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LaunchUndubbing(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(JpIsoFile) && !string.IsNullOrWhiteSpace(UsIsoFile))
            {
                Zero2FileImporter.LaunchUndub(UsIsoFile, JpIsoFile);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var usFileDialog = new OpenFileDialog
            {
                Filter = "iso files (*.iso)|*.iso|All files (*.*)|*.*", 
                Title = "Select the USA ISO"
            };

            if (usFileDialog.ShowDialog() == true)
            {
                UsIsoFile = usFileDialog.FileName;
            }

            var jpFileDialog = new OpenFileDialog
            {
                Filter = "iso files (*.iso)|*.iso|All files (*.*)|*.*", 
                Title = "Select the USA ISO"
            };

            if (jpFileDialog.ShowDialog() == true)
            {
                JpIsoFile = jpFileDialog.FileName;
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Console.WriteLine("Hi!");
            }
        }
    }
}
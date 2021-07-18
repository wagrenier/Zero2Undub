using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private bool IsUndubLaunched { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UndubGame(object sender, DoWorkEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(JpIsoFile) || string.IsNullOrWhiteSpace(UsIsoFile))
            {
                MessageBox.Show("Please select the files before!", "PS2 Fatal Frame 2 Undubber");
                return;
            }
            
            MessageBox.Show("Copying the US ISO, this may take a few minutes!", "PS2 Fatal Frame 2 Undubber");
            IsUndubLaunched = true;
                
            (sender as BackgroundWorker)?.ReportProgress(10);
            var importer = new Zero2FileImporter(UsIsoFile, JpIsoFile);
                
            var task = Task.Factory.StartNew(() =>
            {
                importer.UndubGame();
            });
                
            while (!importer.IsCompleted)
            {
                (sender as BackgroundWorker)?.ReportProgress(100 * importer.UndubbedFiles / (Ps2Constants.NumberFiles));
                Thread.Sleep(100);
            }
            
            (sender as BackgroundWorker)?.ReportProgress(100 * importer.UndubbedFiles / (Ps2Constants.NumberFiles));
            MessageBox.Show("All Done! Enjoy the game :D", "PS2 Fatal Frame 2 Undubber");
        }

        private void LaunchUndubbing(object sender, EventArgs e)
        {
            if (IsUndubLaunched)
            {
                return;
            }

            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            worker.DoWork += UndubGame;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
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
                Title = "Select the JP ISO"
            };

            if (jpFileDialog.ShowDialog() == true)
            {
                JpIsoFile = jpFileDialog.FileName;
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Zero2UndubProcess;
using Zero2UndubProcess.Importer;

namespace Zero2Undub
{
    public partial class MainWindow : Window
    {
        private const string WindowName = "PS2 Fatal Frame 2 Undubber";
        private string OriginIsoFile { get; set; }
        private string TargetIsoFile { get; set; }
        private bool IsUndubLaunched { get; set; }
        private UndubOptions UndubOptions { get; set; } = new UndubOptions();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UndubGame(object sender, DoWorkEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(OriginIsoFile) || string.IsNullOrWhiteSpace(TargetIsoFile))
            {
                MessageBox.Show("Please select the files before!", WindowName);
                return;
            }
            
            MessageBox.Show("Copying the US or EU ISO, this may take a few minutes!", WindowName);
            IsUndubLaunched = true;
                
            (sender as BackgroundWorker)?.ReportProgress(10);

            try
            {
                var importer = new ZeroFileImporter(UndubOptions, OriginIsoFile, TargetIsoFile);

                var task = Task.Factory.StartNew(() => { importer.RestoreGame(); });

                while (!importer.InfoReporterUi.IsCompleted)
                {
                    (sender as BackgroundWorker)?.ReportProgress(100 * importer.InfoReporterUi.FilesCompleted /
                                                                 importer.InfoReporterUi.TotalFiles);
                    Thread.Sleep(100);
                }

                (sender as BackgroundWorker)?.ReportProgress(100);

                if (!importer.InfoReporterUi.IsSuccess)
                {
                    MessageBox.Show(
                        $"The program failed with the following message: {importer.InfoReporterUi.ErrorMessage}",
                        WindowName);
                    return;
                }

                MessageBox.Show("All Done! Enjoy the game :D", WindowName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void CbSafeUndubChecked(object sender, RoutedEventArgs e)
        {
            UndubOptions.SafeUndub = safeUndubBtn.IsChecked == true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var usFileDialog = new OpenFileDialog
            {
                Filter = "iso files (*.iso)|*.iso|All files (*.*)|*.*",
                Title = "Select the USA or EU ISO"
            };

            if (usFileDialog.ShowDialog() == true)
            {
                TargetIsoFile = usFileDialog.FileName;
            }

            var jpFileDialog = new OpenFileDialog
            {
                Filter = "iso files (*.iso)|*.iso|All files (*.*)|*.*",
                Title = "Select the JP ISO"
            };

            if (jpFileDialog.ShowDialog() == true)
            {
                OriginIsoFile = jpFileDialog.FileName;
            }
        }
    }
}
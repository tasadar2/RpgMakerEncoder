using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using RpgMakerEncoder.Configuration;
using RpgMakerEncoder.Encoding;
using RpgMakerEncoder.RpgMaker;

namespace RpgMakerEncoder
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Encoding.RpgMakerEncoder _encoder;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Load();

            if (string.IsNullOrEmpty(Settings.GamePath))
            {
                Settings.GamePath = RpgMakerHelper.LocateGamePath();
            }
            if (string.IsNullOrEmpty(Settings.SourcePath))
            {
                Settings.SourcePath = Path.GetDirectoryName(Settings.GamePath);
            }

            GameDirectoryText.Text = Settings.GamePath;
            SourceDirectoryText.Text = Settings.SourcePath;

            _encoder = new Encoding.RpgMakerEncoder(Settings.GamePath, Settings.SourcePath)
            {
                EncoderOptions = new RubyEncoderOptions
                {
                    UserEncoders = RpgMakerHelper.UserDefinitionEncoders()
                },
                DecoderOptions = new RubyDecoderOptions
                {
                    UserDecoders = RpgMakerHelper.UserDefinitionDecoders()
                }
            };
            _encoder.OperationsProgress += EncoderOperationsProgress;
            _encoder.OperationsComplete += EncoderOperationsComplete;
        }

        private void EncoderOperationsProgress(object sender, OperationsProgressEventArgs e)
        {
            Dispatcher.Invoke(() => StatusChanged(e.Count, e.Total));
        }

        private void EncoderOperationsComplete(object sender, OperationsCompleteEventArgs e)
        {
            Dispatcher.Invoke(() => OperationsComplete("Finished"));
        }

        private void StatusChanged(int count, int total)
        {
            if (total > StatusBar.Maximum)
            {
                StatusBar.Maximum = total;
            }

            if (count > StatusBar.Value)
            {
                StatusBar.Value = count;
            }
        }

        private void StartOperations(string message)
        {
            StatusText.Text = message;
            StatusBar.Value = 0;
            StatusBar.Maximum = 0;
            buttonEncode.IsEnabled = false;
            buttonDecode.IsEnabled = false;
            GameDirectoryButton.IsEnabled = false;
            SourceDirectoryButton.IsEnabled = false;
        }

        private void OperationsComplete(string message)
        {
            StatusText.Text = message;
            StatusBar.Value = StatusBar.Maximum;
            buttonEncode.IsEnabled = true;
            buttonDecode.IsEnabled = true;
            GameDirectoryButton.IsEnabled = true;
            SourceDirectoryButton.IsEnabled = true;
        }

        private void buttonDecode_Click(object sender, RoutedEventArgs e)
        {
            StartOperations("Decoding...");
            _encoder.Decode();
        }

        private void buttonEncode_Click(object sender, RoutedEventArgs e)
        {
            StartOperations("Encoding...");
            _encoder.Encode();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Save();
        }

        private void GameDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = GameDirectoryText.Text,
                ShowNewFolderButton = true,
                Description = "Select the Game directory path.\r\n" +
                              "Game - ./Source/Game/\r\n" +
                              "Source - ./Source/"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.GamePath = dialog.SelectedPath;
                GameDirectoryText.Text = Settings.GamePath;
            }
        }

        private void SourceDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = SourceDirectoryText.Text,
                ShowNewFolderButton = true,
                Description = "Select the Source directory path.\r\n" +
                              "Game - ./Source/Game/\r\n" +
                              "Source - ./Source/"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.SourcePath = dialog.SelectedPath;
                SourceDirectoryText.Text = Settings.SourcePath;
            }
        }
    }
}
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.IO;

namespace Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process _apiProc; // a process for handling asp net core web api app
        private Process _tunnelProc; // a process for handling cloudflare tunnel

        // target folder for files. By default it's located in the root of the asp.net project and called ReceivedFiles
        private string _targetFolder = Path.Combine(@"C:\FileGet", "Files");

        public MainWindow()
        {
            InitializeComponent();
            PathDisplay.Text = _targetFolder;
        }

        private void UpdateApiConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            // Читаем текущий файл, если он есть
            var jsonString = File.Exists(configPath) ? File.ReadAllText(configPath) : "{}";
            var configNode = System.Text.Json.Nodes.JsonNode.Parse(jsonString);

            // Обновляем только нашу секцию
            configNode["Printing"] = new System.Text.Json.Nodes.JsonObject
            {
                ["TargetFolder"] = _targetFolder
            };

            File.WriteAllText(configPath, configNode.ToString());
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                _targetFolder = dialog.FolderName;
                PathDisplay.Text = _targetFolder;
                UpdateApiConfig();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // 1. Запускаем API (предположим, что exe лежит в той же папке)
            _apiProc = Process.Start(new ProcessStartInfo
            {
                FileName = "LocalServer.exe",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            // 2. Запускаем Cloudflare
            _tunnelProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cloudflared.exe",
                    Arguments = "tunnel --url http://localhost:5000",
                    RedirectStandardError = true, // Cloudflare пишет логи в Error stream
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _tunnelProc.ErrorDataReceived += (s, ev) => {
                if (ev.Data != null && ev.Data.Contains("trycloudflare.com"))
                {
                    // Ищем ссылку в тексте через Regex или простой поиск
                    var words = ev.Data.Split(' ');
                    foreach (var word in words)
                    {
                        if (word.Contains("trycloudflare.com"))
                        {
                            Dispatcher.Invoke(() => UrlDisplay.Text = word);
                        }
                    }
                }
            };

            _tunnelProc.Start();
            _tunnelProc.BeginErrorReadLine();

            BtnStart.Visibility = Visibility.Collapsed;
            BtnStop.Visibility = Visibility.Visible;
            StatusText.Text = "Статус: РАБОТАЕТ";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _apiProc?.Kill();
            _tunnelProc?.Kill();

            BtnStart.Visibility = Visibility.Visible;
            BtnStop.Visibility = Visibility.Collapsed;
            StatusText.Text = "Статус: Остановлен";
            UrlDisplay.Text = "";
        }
    }
}
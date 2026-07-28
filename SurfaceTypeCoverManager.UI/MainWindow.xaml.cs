using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SurfaceTypeCoverManager.UI
{
    public partial class MainWindow : Window
    {
        private Process _webServerProcess;

        public MainWindow()
        {
            InitializeComponent();
            StartWebServer();
        }

        private void StartWebServer()
        {
            // Start the ASP.NET Core Web API backend in the background
            try
            {
                var basePath = System.AppDomain.CurrentDomain.BaseDirectory;
                // Since this runs in bin/Debug/net8.0-windows..., we need to point it to the Web project's output
                // But for development, we can use the dotnet run command or point directly to the Web dll.
                // Assuming the web app compiles to its own bin folder, we can just run dotnet on the Web dll,
                // or just start the web executable. We'll start the web executable if it exists.
                
                string webExePath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "..", "SurfaceTypeCoverManager.Web", "bin", "Debug", "net8.0-windows10.0.19041.0", "SurfaceTypeCoverManager.Web.exe"));

                if (File.Exists(webExePath))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = webExePath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(webExePath)
                    };
                    _webServerProcess = Process.Start(startInfo);
                }
            }
            catch
            {
                // Ignore for now, WebView2 will just show a failed to connect if server isn't running
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_webServerProcess != null && !_webServerProcess.HasExited)
            {
                _webServerProcess.Kill();
            }
            this.Close();
        }
    }
}
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SurfaceTypeCoverManager.Web;

namespace SurfaceTypeCoverManager.UI
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _webServerCts;
        private Thread? _webServerThread;

        public MainWindow()
        {
            InitializeComponent();
            StartEmbeddedWebServer();
        }

        private void StartEmbeddedWebServer()
        {
            _webServerCts = new CancellationTokenSource();

            // Resolve wwwroot from the assembly's base directory
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var wwwroot = Path.Combine(baseDir, "wwwroot");

            // Run Kestrel on a dedicated background thread to avoid WPF STA conflicts
            _webServerThread = new Thread(() =>
            {
                try
                {
                    var app = WebServerHost.BuildApp(wwwroot);
                    app.RunAsync("http://localhost:5000").GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Web server error: {ex.Message}");
                }
            })
            {
                IsBackground = true,
                Name = "KestrelWebServer"
            };
            _webServerThread.Start();
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
            _webServerCts?.Cancel();
            this.Close();
        }
    }
}
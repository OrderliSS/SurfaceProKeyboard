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

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var wwwroot = Path.Combine(baseDir, "wwwroot");

            Task.Run(async () =>
            {
                try
                {
                    var app = WebServerHost.BuildApp(wwwroot);
                    app.Urls.Clear();
                    app.Urls.Add("http://127.0.0.1:0"); // Bind to dynamic port
                    
                    await app.StartAsync(_webServerCts.Token);
                    
                    var address = app.Urls.FirstOrDefault() ?? "http://127.0.0.1:5000";

                    // Safely initialize WebView2 on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InitializeWebView(address);
                    });

                    await app.WaitForShutdownAsync(_webServerCts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Web server error: {ex.Message}");
                }
            });
        }

        private async void InitializeWebView(string url)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
                webView.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 Init error: {ex.Message}");
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
            _webServerCts?.Cancel();
            this.Close();
        }
    }
}
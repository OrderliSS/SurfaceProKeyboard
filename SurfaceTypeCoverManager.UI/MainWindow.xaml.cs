using System;
using System.Windows;
using System.Windows.Interop;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.UI.ViewModels;

namespace SurfaceTypeCoverManager.UI
{
    public partial class MainWindow : Window
    {
        private readonly IDeviceWatcherService _deviceWatcher;
        private readonly IKeyboardService _keyboardService;

        public MainWindow(MainViewModel mainVM, IDeviceWatcherService deviceWatcher, IKeyboardService keyboardService)
        {
            InitializeComponent();
            DataContext = mainVM;
            _deviceWatcher = deviceWatcher;
            _keyboardService = keyboardService;

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                _deviceWatcher.StartMonitoring(hwnd);
                _keyboardService.StartKeyHook(hwnd);
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _deviceWatcher.StopMonitoring();
            _keyboardService.StopKeyHook();
        }
    }
}
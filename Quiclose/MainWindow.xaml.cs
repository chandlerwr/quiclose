using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quiclose {

    [Flags]
    internal enum HotKeyModifier {
        MOD_ALT = 0x0001,
        MOD_CONTROL = 0x0002,
        MOD_NOREPEAT = 0x4000,
        MOD_SHIFT = 0x0004,
        MOD_WIN = 0x0008,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private const string
            CHECKMARK_RESOURCE = "/Quiclose;component/shell32-296.png",
            X_RESOURCE = "/Quiclose;component/shell32-131.png";

        private bool _keepOpen = true;

        private Closer? _closer = null;

        public MainWindow () {
            InitializeComponent();
            DataContext = this;

            _poller = new Timer(PollerTick, null, 0, 50);
        }

        // TODO: persistence
        private string _targetName = "GTA5";
        public string TargetName {
            get => _targetName;
            set {
                if (value == _targetName) return;
                _closer = null;
                _targetName = value; // doesn't notify... only modified from UI
            }
        }

        private string _targetStatus = X_RESOURCE;
        public string TargetStatus {
            get => _targetStatus;
            set {
                if (value == _targetStatus) return;
                _targetStatus = value;
                NotifyPropertyChanged();
            }
        }

        #region Global Hotkey
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey ([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);
        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey ([In] IntPtr hWnd, [In] int id);

        private HwndSource? _source;
        private const int HOTKEY_ID = 9000;

        // https://stackoverflow.com/a/11378213
        protected override void OnSourceInitialized (EventArgs e) {
            base.OnSourceInitialized(e);
            //var helper = new WindowInteropHelper(this);
            //_source = HwndSource.FromHwnd(helper.Handle);
            //_source.AddHook(HwndHook);
            //RegisterHotKey();
        }

        protected override void OnClosed (EventArgs e) {
            //_source?.RemoveHook(HwndHook);
            //_source = null;
            //UnregisterHotKey();
            _poller.Dispose();
            base.OnClosed(e);
        }

        private void RegisterHotKey () {
            var helper = new WindowInteropHelper(this);
            // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            const uint VK_P = 0x50;
            const uint VK_F11 = 0x7A;
            const uint VK_PAUSE = 0x13;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, (uint) HotKeyModifier.MOD_NOREPEAT, VK_F11)) {
                MessageBox.Show(this, "Failed to setup global hotkey.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _keepOpen = false;
                Close();
            }
        }

        private void UnregisterHotKey () {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            switch (msg) {
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region Key Polling
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState ([In] int vKey);

        private readonly Timer _poller;
        private const int VK_PAUSE = 0x13;
        private bool _keyState = false; // BELONGS TO POLLER THREAD
        private void PollerTick (object? state) {
            // https://stackoverflow.com/a/48325924

            short keyState = GetAsyncKeyState(VK_PAUSE);
            // Check if the MSB is set. If so, then the key is pressed.
            bool isPressed = ((keyState >> 15) & 0x0001) == 0x0001;

            if (_keyState == isPressed) return;
            _keyState = isPressed;
            if (isPressed) Dispatcher.Invoke(OnHotKeyPressed);

        }
        #endregion

        #region Tray Functionality
        protected override void OnClosing (CancelEventArgs e) {
            e.Cancel = _keepOpen;
            base.OnClosing(e);
            if (_keepOpen) Hide();
        }

        private void TrayOpen (object sender, RoutedEventArgs e) {
            Show();
        }

        private void TrayQuit (object sender, RoutedEventArgs e) {
            _keepOpen = false;
            Close();
        }

        private void TrayDoubleClick (object sender, RoutedEventArgs e) {
            Show();
        }
        #endregion

        #region Close App Functionality
        private void OnCheckClick (object sender, RoutedEventArgs e) {
            if (_closer is null) _closer = new Closer(_targetName);
            else _closer.Poll();
            TargetStatus = _closer.HasProcess ? CHECKMARK_RESOURCE : X_RESOURCE;
            
            _check.Visibility = Visibility.Visible;
        }

        private void Window_Deactivated (object sender, EventArgs e) {
            _check.Visibility = Visibility.Hidden;
        }

        private void OnHotKeyPressed () {
            if (_closer is null) _closer = new Closer(_targetName);
            else _closer.Poll();
            if (!_closer.HasProcess) {
                _taskbar.ShowBalloonTip("Error", "Couldn't find application to close", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                return;
            }
            _closer.Close();
            _taskbar.ShowBalloonTip("Success", "Closed app", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
        #endregion

    }
}
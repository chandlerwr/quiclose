using System.ComponentModel;
using System.Diagnostics;
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
    public partial class MainWindow : Window {

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey ([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey ([In] IntPtr hWnd, [In] int id);

        private HwndSource? _source;
        private const int HOTKEY_ID = 9000;

        private bool _keepOpen = true;

        private Closer? _closer = null;

        public MainWindow () {
            InitializeComponent();

            var ps = Process.GetProcessesByName("Notepad");
            string o = ps.Length != 0 ? ("Found: " + string.Join(", ", ps.Select(p => p.Id))) : "NONE FOUND!";

            Debug.WriteLine($"\n*****\n\n{o}\n\n*****\n");

            if (ps.Length == 1) ps[0].Kill();

        }

        #region Global Hotkey
        // https://stackoverflow.com/a/11378213
        protected override void OnSourceInitialized (EventArgs e) {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed (EventArgs e) {
            _source?.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey () {
            var helper = new WindowInteropHelper(this);
            // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            const uint VK_P = 0x50;
            const uint VK_PAUSE = 0x13;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, (uint) HotKeyModifier.MOD_NOREPEAT, VK_PAUSE)) {
                MessageBox.Show(this, "Failed to setup global hotkey.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _keepOpen = false;
                Close();
            }
        }

        private void UnregisterHotKey () {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private void OnHotKeyPressed () {
            _taskbar.ShowBalloonTip("Error", "Couldn't find application to close", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
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
    }
}
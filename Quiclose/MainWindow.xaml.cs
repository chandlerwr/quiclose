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

namespace Quiclose;

/// <summary>
/// Main window of Quiclose.
/// </summary>
public partial class MainWindow : Window {

    private bool _keepOpen = true;


    public MainWindow () {
        InitializeComponent();
        ViewModel = new QuicloseVM();
        DataContext = ViewModel;

        ViewModel.TriedClosingApp += OnTriedClosingApp;
    }

    private void OnTriedClosingApp (bool closed) {
        if (closed) _taskbar.ShowBalloonTip("Success", "Closed app", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        else _taskbar.ShowBalloonTip("Error", "Couldn't find application to close", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
    }

    internal QuicloseVM ViewModel { get; }


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

}

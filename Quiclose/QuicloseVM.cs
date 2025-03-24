using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Quiclose;

/// <summary>
/// Main viewmodel of application.
/// </summary>
internal partial class QuicloseVM : ObservableValidator {

    private const string PERSISTENCE_KEY_NAME = "AppName";
    private const string PERSISTENCE_KEY_KEYBIND = "Keybind";

    private readonly KeyPoller _poller;

    [ObservableProperty]
    [MinLength(1)]
    [Required]
    [NotifyDataErrorInfo]
    private string _appName = "GTA5_Enhanced";
    [ObservableProperty]
    private bool? _appRunning = null;
    [ObservableProperty]
    private VK _keybind = VK.PAUSE;


    public QuicloseVM () {
        if (Persistence.TryLoadValue(PERSISTENCE_KEY_NAME, out string? val)) AppName = val;
        if (Persistence.TryLoadValue(PERSISTENCE_KEY_KEYBIND, out VK? key) && key is not null) Keybind = key.Value;

        // Dirty way of passing what I know to be the UI dispatch to poller
        // For a facade of thread-safety
        _poller = new(Dispatcher.CurrentDispatcher);
        _poller.KeyPressed += (_, _) => {
            var success = TryCloseApp();
            TriedClosingApp?.Invoke(success);
        };
    }


    /// <summary>
    /// Invoked when keybind to close app is pressed. Parameter passed is whether or not the app was closed.
    /// </summary>
    /// <remarks>
    /// Called on the thread this VM was created on.
    /// </remarks>
    public event Action<bool>? TriedClosingApp;


    public bool HasNoErrors => !HasErrors;


    [RelayCommand(CanExecute = nameof(HasNoErrors))]
    private void CheckAppRunning () {
        AppRunning = TryGetSingleProcess(out _);
    }

    [RelayCommand]
    private void ResetAppRunning () {
        AppRunning = null;
    }

    [RelayCommand]
    private void Shutdown () {
        Persistence.SetValue(PERSISTENCE_KEY_KEYBIND, _poller.Key);
        Persistence.SetValue(PERSISTENCE_KEY_NAME, AppName);
        Persistence.SaveValues();
        _poller.Dispose();
    }

    partial void OnAppNameChanged (string value) {
        if (AppRunning is not null) ResetAppRunning();
    }

    partial void OnKeybindChanged (VK value) {
        _poller.Key = value;
    }

    private bool TryCloseApp () {
        if (!TryGetSingleProcess(out var proc)) return false;
        proc.Kill();
        AppRunning = null;
        return true;
    }

    private bool TryGetSingleProcess ([NotNullWhen(true)] out Process? proc) {
        var ret = Process.GetProcessesByName(AppName);
        if (ret.Length != 1) {
            proc = null;
            return false;
        }
        proc = ret[0];
        return true;
    }

}


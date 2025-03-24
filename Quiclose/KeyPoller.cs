
using System.Windows.Threading;

namespace Quiclose;

/// <summary>
/// Listens for key presses via Win32 polling.
/// </summary>
internal sealed class KeyPoller : IDisposable {

    private readonly Timer _poller;
    private readonly Dispatcher _dispatcher;
    private bool _keyState = false; // BELONGS TO POLLER THREAD


    public KeyPoller () : this(Dispatcher.CurrentDispatcher) { }

    public KeyPoller (Dispatcher dispatcher) {
        _poller = new(Tick, null, 0, 50);
        _dispatcher = dispatcher;
    }


    public event EventHandler? KeyPressed;


    /// <summary>
    /// The key to listen for which invokes <see cref="KeyPressed"/>.
    /// </summary>
    public VK Key { get; set; } = VK.PAUSE;  // shouldn't result in race... ignoring possible threading issues for now...


    public void Dispose () {
        _poller.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Tick (object? state) {

        bool isPressed = Native.IsKeyPressed(Key);

        if (_keyState == isPressed) return;
        _keyState = isPressed;
        if (isPressed) _dispatcher.Invoke(() => KeyPressed?.Invoke(this, EventArgs.Empty));

    }

}


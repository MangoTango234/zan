using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Zan.Input;

/// <summary>
/// Registers system-wide hotkeys via the Win32 RegisterHotKey API and dispatches
/// WM_HOTKEY messages to handlers. Uses a message-only window so the tray app
/// needs no visible main window. Must be constructed and used on the UI thread.
/// </summary>
internal sealed class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_NOREPEAT = 0x4000;
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    private readonly HwndSource _source;
    private readonly Dictionary<int, Action> _handlers = new();
    private int _nextId = 1;

    public HotkeyService()
    {
        var parameters = new HwndSourceParameters("ZanHotkeyWindow")
        {
            ParentWindow = HWND_MESSAGE, // message-only window: no UI, just receives messages
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    /// <summary>Registers a hotkey. Returns its id, or -1 if it could not be registered
    /// (e.g. the combo is already taken by another application).</summary>
    public int Register(HotkeyCombo combo, Action onPressed)
    {
        var id = _nextId++;
        if (!RegisterHotKey(_source.Handle, id, combo.Win32Modifiers | MOD_NOREPEAT, combo.VirtualKey))
            return -1;
        _handlers[id] = onPressed;
        return id;
    }

    public void UnregisterAll()
    {
        foreach (var id in _handlers.Keys)
            UnregisterHotKey(_source.Handle, id);
        _handlers.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && _handlers.TryGetValue(wParam.ToInt32(), out var handler))
        {
            handler();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

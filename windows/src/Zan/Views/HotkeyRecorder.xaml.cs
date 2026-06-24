using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zan.Input;

namespace Zan.Views;

/// <summary>
/// A small control to capture a global hotkey. Click "Set", press a modifier
/// combo (at least one of Ctrl/Alt/Win plus a key), and it stores the result as
/// a string like "Ctrl+Alt+P". "Clear" removes the binding.
/// </summary>
public partial class HotkeyRecorder : UserControl
{
    private const string NonePlaceholder = "(none)";
    private const string ListeningPlaceholder = "Press keys…";

    private bool _listening;
    private string _hotkey = "";

    public event EventHandler? HotkeyChanged;

    public HotkeyRecorder()
    {
        InitializeComponent();
        Display.Text = NonePlaceholder;
    }

    /// <summary>The captured hotkey string ("" when unassigned).</summary>
    public string Hotkey
    {
        get => _hotkey;
        set
        {
            _hotkey = value ?? "";
            if (!_listening)
                Display.Text = _hotkey.Length == 0 ? NonePlaceholder : _hotkey;
        }
    }

    private void Set_Click(object sender, RoutedEventArgs e)
    {
        _listening = true;
        Display.Text = ListeningPlaceholder;
        Display.Focus();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _listening = false;
        _hotkey = "";
        Display.Text = NonePlaceholder;
        HotkeyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Display_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_listening)
            EndListening();
    }

    private void Display_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_listening) return;
        e.Handled = true;

        // When Alt is held, the real key arrives via SystemKey.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            EndListening();
            return;
        }

        if (IsModifierKey(key))
            return; // wait for a non-modifier key

        var mods = Keyboard.Modifiers;
        if (mods == ModifierKeys.None || mods == ModifierKeys.Shift)
            return; // require at least one of Ctrl/Alt/Win so the combo is global-safe

        _hotkey = new HotkeyCombo { Modifiers = mods, Key = key }.ToString();
        EndListening();
        HotkeyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndListening()
    {
        _listening = false;
        Display.Text = _hotkey.Length == 0 ? NonePlaceholder : _hotkey;
    }

    private static bool IsModifierKey(Key key) =>
        key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift
            or Key.LWin or Key.RWin
            or Key.System;
}

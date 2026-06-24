using System.Windows.Input;

namespace Zan.Input;

/// <summary>
/// A modifier+key combination, with parsing/formatting to a stable string like
/// "Ctrl+Alt+P" and conversion to the Win32 values RegisterHotKey expects.
/// </summary>
public sealed class HotkeyCombo
{
    // Win32 fsModifiers (RegisterHotKey).
    private const uint MOD_ALT = 0x1;
    private const uint MOD_CONTROL = 0x2;
    private const uint MOD_SHIFT = 0x4;
    private const uint MOD_WIN = 0x8;

    public ModifierKeys Modifiers { get; init; }
    public Key Key { get; init; }

    public bool HasModifier => Modifiers != ModifierKeys.None;

    public uint Win32Modifiers
    {
        get
        {
            uint m = 0;
            if (Modifiers.HasFlag(ModifierKeys.Alt)) m |= MOD_ALT;
            if (Modifiers.HasFlag(ModifierKeys.Control)) m |= MOD_CONTROL;
            if (Modifiers.HasFlag(ModifierKeys.Shift)) m |= MOD_SHIFT;
            if (Modifiers.HasFlag(ModifierKeys.Windows)) m |= MOD_WIN;
            return m;
        }
    }

    public uint VirtualKey => (uint)KeyInterop.VirtualKeyFromKey(Key);

    public override string ToString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(Key.ToString());
        return string.Join("+", parts);
    }

    public static bool TryParse(string? text, out HotkeyCombo combo)
    {
        combo = new HotkeyCombo();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return false;

        var mods = ModifierKeys.None;
        var key = Key.None;

        foreach (var part in parts)
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    mods |= ModifierKeys.Control;
                    break;
                case "alt":
                    mods |= ModifierKeys.Alt;
                    break;
                case "shift":
                    mods |= ModifierKeys.Shift;
                    break;
                case "win":
                case "windows":
                case "cmd":
                    mods |= ModifierKeys.Windows;
                    break;
                default:
                    if (!Enum.TryParse(part, ignoreCase: true, out key))
                        return false;
                    break;
            }
        }

        if (key == Key.None) return false;
        combo = new HotkeyCombo { Modifiers = mods, Key = key };
        return true;
    }
}

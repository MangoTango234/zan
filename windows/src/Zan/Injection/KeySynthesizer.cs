using System.Runtime.InteropServices;

namespace Zan.Injection;

/// <summary>
/// Synthesizes Ctrl+C / Ctrl+V via SendInput, and waits for physical modifier
/// keys to be released first. The wait is essential: if Ctrl/Alt are still held
/// from the triggering hotkey when we synthesize the keystroke, the OS sees a
/// contaminated combo and the copy/paste fails. Mirrors the macOS KeySynthesizer.
/// </summary>
internal static class KeySynthesizer
{
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_MENU = 0x12;   // Alt
    private const ushort VK_SHIFT = 0x10;
    private const ushort VK_LWIN = 0x5B;
    private const ushort VK_RWIN = 0x5C;
    private const ushort VK_C = 0x43;
    private const ushort VK_V = 0x56;

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static async Task WaitForModifiersReleasedAsync(int timeoutMs = 1000)
    {
        var start = Environment.TickCount;
        while (AnyModifierDown())
        {
            if (Environment.TickCount - start > timeoutMs)
                break;
            await Task.Delay(15);
        }
    }

    public static void SendCopy() => SendCtrlChord(VK_C);

    public static void SendPaste() => SendCtrlChord(VK_V);

    private static void SendCtrlChord(ushort key)
    {
        var inputs = new[]
        {
            MakeKey(VK_CONTROL, false),
            MakeKey(key, false),
            MakeKey(key, true),
            MakeKey(VK_CONTROL, true),
        };
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static bool AnyModifierDown() =>
        IsDown(VK_CONTROL) || IsDown(VK_MENU) || IsDown(VK_SHIFT) || IsDown(VK_LWIN) || IsDown(VK_RWIN);

    private static bool IsDown(ushort vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    private static INPUT MakeKey(ushort vk, bool keyUp) => new()
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = vk,
                dwFlags = keyUp ? KEYEVENTF_KEYUP : 0,
            },
        },
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}

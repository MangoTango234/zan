using System.Threading;
using System.Windows;

namespace Zan.Injection;

/// <summary>
/// Clipboard read/write with snapshot+restore so the user's clipboard survives a
/// copy/paste round-trip. The Win32 clipboard can be briefly locked by another
/// process, so writes retry. Must be called on the STA (UI) thread.
/// </summary>
internal static class ClipboardHelper
{
    public static string? SnapshotText()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetText()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }
        catch
        {
            return null;
        }
    }

    public static void SetText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Clear();
            return;
        }
        TryRun(() => Clipboard.SetText(text));
    }

    public static void Clear() => TryRun(Clipboard.Clear);

    public static void RestoreText(string? saved)
    {
        if (saved == null)
            Clear();
        else
            SetText(saved);
    }

    private static void TryRun(Action action)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch
            {
                Thread.Sleep(20);
            }
        }
    }
}

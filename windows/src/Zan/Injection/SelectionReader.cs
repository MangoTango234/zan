namespace Zan.Injection;

/// <summary>
/// Reads the current selection by synthesizing Ctrl+C and reading the clipboard,
/// then restoring the prior clipboard. Mirrors the macOS SelectionReader (Cmd+C).
/// </summary>
internal static class SelectionReader
{
    public static async Task<string?> ReadSelectionAsync()
    {
        await KeySynthesizer.WaitForModifiersReleasedAsync();

        var saved = ClipboardHelper.SnapshotText();
        ClipboardHelper.Clear();

        KeySynthesizer.SendCopy();
        await Task.Delay(120); // let the foreground app place the selection on the clipboard

        var text = ClipboardHelper.GetText();
        ClipboardHelper.RestoreText(saved);
        return text;
    }
}

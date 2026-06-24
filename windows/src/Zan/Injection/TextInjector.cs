namespace Zan.Injection;

/// <summary>
/// Inserts text at the cursor by placing it on the clipboard and synthesizing
/// Ctrl+V, then restoring the prior clipboard. Mirrors the macOS TextInjector (Cmd+V).
/// </summary>
internal static class TextInjector
{
    public static async Task PasteAsync(string text)
    {
        await KeySynthesizer.WaitForModifiersReleasedAsync();

        var saved = ClipboardHelper.SnapshotText();
        ClipboardHelper.SetText(text);
        await Task.Delay(60); // let the clipboard settle before pasting

        KeySynthesizer.SendPaste();
        await Task.Delay(120); // let the foreground app consume the paste before we restore

        ClipboardHelper.RestoreText(saved);
    }
}

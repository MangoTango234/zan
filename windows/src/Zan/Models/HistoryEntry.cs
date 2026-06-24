using System.Text.Json.Serialization;

namespace Zan.Models;

/// <summary>
/// One past dictation or action, shown in the in-app history. Mirrors the macOS
/// HistoryStore entries.
/// </summary>
public sealed class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    /// <summary>"action" or "dictation".</summary>
    public string Kind { get; set; } = "action";

    /// <summary>Action name, or "Dictation".</summary>
    public string Title { get; set; } = "";

    public string Input { get; set; } = "";
    public string Output { get; set; } = "";
}

/// <summary>The on-disk history file (%APPDATA%\Zan\history.json).</summary>
public sealed class HistoryDocument
{
    [JsonPropertyName("entries")]
    public List<HistoryEntry> Entries { get; set; } = new();
}

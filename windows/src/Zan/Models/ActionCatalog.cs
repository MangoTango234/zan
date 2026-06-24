using System.Text.Json.Serialization;

namespace Zan.Models;

/// <summary>
/// A single built-in or user action. Mirrors the macOS unified Action and the
/// schema in shared/actions.json. <see cref="Engine"/> is "ai" | "prefix";
/// <see cref="Output"/> is "replaceSelection" | "popup" | "copy".
/// </summary>
public sealed class ActionItem
{
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Engine { get; set; } = "ai";
    public string Output { get; set; } = "replaceSelection";

    /// <summary>LLM prompt for engine "ai".</summary>
    public string? Prompt { get; set; }

    /// <summary>Fixed string prepended for engine "prefix".</summary>
    public string? Prefix { get; set; }
}

/// <summary>
/// The shared/actions.json document: the dictation cleanup prompt plus the
/// catalog of built-in actions.
/// </summary>
public sealed class ActionCatalog
{
    [JsonPropertyName("dictationCleanupPrompt")]
    public string DictationCleanupPrompt { get; set; } = "";

    [JsonPropertyName("actions")]
    public List<ActionItem> Actions { get; set; } = new();
}

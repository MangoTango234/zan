using System.Text.Json.Serialization;

namespace Zan.Models;

/// <summary>
/// One unified action. Mirrors the macOS Action (Zan/Sources/Models/Action.swift)
/// and the schema in shared/actions.json. <see cref="Engine"/> is "ai" | "prefix";
/// <see cref="Output"/> is "replaceSelection" | "popup" | "copy".
///
/// <see cref="ShortcutKey"/> and <see cref="IsBuiltIn"/> are not part of the
/// shared catalog file; they are assigned when seeding and persisted in the
/// per-user actions file. Built-ins keep stable shortcut keys (matched to macOS)
/// so saved hotkeys survive upgrades.
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

    /// <summary>Stable identity used to bind/persist this action's hotkey.</summary>
    public string ShortcutKey { get; set; } = "";

    /// <summary>Global hotkey, formatted like "Ctrl+Alt+P". Empty = unassigned.</summary>
    public string Hotkey { get; set; } = "";

    public bool IsBuiltIn { get; set; }

    public ActionItem Clone() => new()
    {
        Name = Name,
        Detail = Detail,
        Engine = Engine,
        Output = Output,
        Prompt = Prompt,
        Prefix = Prefix,
        ShortcutKey = ShortcutKey,
        Hotkey = Hotkey,
        IsBuiltIn = IsBuiltIn,
    };
}

/// <summary>
/// The shared/actions.json document: the dictation cleanup prompt plus the
/// catalog of built-in actions. This is the cross-platform seed.
/// </summary>
public sealed class ActionCatalog
{
    [JsonPropertyName("dictationCleanupPrompt")]
    public string DictationCleanupPrompt { get; set; } = "";

    [JsonPropertyName("actions")]
    public List<ActionItem> Actions { get; set; } = new();
}

/// <summary>
/// The per-user actions file persisted to %APPDATA%\Zan\actions.json. Built-ins
/// are seeded from the shared catalog on first run; users may add/edit/delete.
/// </summary>
public sealed class ActionsDocument
{
    [JsonPropertyName("actions")]
    public List<ActionItem> Actions { get; set; } = new();
}

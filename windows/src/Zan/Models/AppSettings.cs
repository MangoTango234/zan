using System.Text.Json.Serialization;

namespace Zan.Models;

/// <summary>
/// Scalar app settings, persisted to %APPDATA%\Zan\settings.json. Mirrors the
/// macOS AppSettings (Zan/Sources/Stores/AppSettings.swift). API keys are NOT
/// here: they go to Windows Credential Manager (see Services/KeyStore).
/// </summary>
public sealed class AppSettings
{
    /// <summary>Which engine transcribes dictation: "openai" (cloud) or "local" (on-device Whisper).</summary>
    public string TranscriptionProvider { get; set; } = "openai";
    public string TranscriptionModel { get; set; } = TranscriptionModels[0];

    /// <summary>On-device Whisper model name (used when provider == local).</summary>
    public string WhisperModel { get; set; } = "base";

    /// <summary>Which provider powers text actions + dictation cleanup: "openai" or "anthropic".</summary>
    public string TextProvider { get; set; } = "openai";
    public string OpenAITextModel { get; set; } = OpenAITextModels[0];
    public string AnthropicTextModel { get; set; } = AnthropicTextModels[0];

    public bool CleanupEnabled { get; set; } = true;

    /// <summary>Editable prompt used to clean up dictation before insertion.</summary>
    public string CleanupPrompt { get; set; } = "";

    /// <summary>Dictation trigger behavior: "toggle" or "holdToTalk".</summary>
    public string DictationMode { get; set; } = "toggle";

    /// <summary>Global hotkey that triggers dictation, formatted like "Ctrl+Alt+Space". Empty = unassigned.</summary>
    public string DictationHotkey { get; set; } = "";

    // Quick-pick presets (free-text too, so newer model IDs can be typed in).
    // Kept in sync with macOS AppSettings.
    [JsonIgnore] public static readonly string[] TranscriptionModels = { "gpt-4o-mini-transcribe", "gpt-4o-transcribe", "whisper-1" };
    [JsonIgnore] public static readonly string[] WhisperModels = { "tiny", "base", "small", "large-v3" };
    [JsonIgnore] public static readonly string[] OpenAITextModels = { "gpt-4o-mini", "gpt-4.1-nano", "gpt-4.1-mini", "gpt-4.1", "gpt-4o" };
    [JsonIgnore] public static readonly string[] AnthropicTextModels = { "claude-haiku-4-5-20251001", "claude-sonnet-4-6", "claude-opus-4-8" };
}

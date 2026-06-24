using Zan.Models;
using Zan.Services;

namespace Zan.Transform;

/// <summary>
/// Builds the text engine for the active provider, pulling the API key from
/// Credential Manager. Returns null with a user-facing message when no key is set.
/// </summary>
internal static class TextEngineFactory
{
    public static ITextTransformer? Create(AppSettings settings, out string error)
    {
        error = string.Empty;

        if (settings.TextProvider == "anthropic")
        {
            var key = KeyStore.AnthropicKey;
            if (string.IsNullOrEmpty(key))
            {
                error = "Add your Anthropic API key in Settings.";
                return null;
            }
            return new AnthropicTextTransformer(key, settings.AnthropicTextModel);
        }

        var openAIKey = KeyStore.OpenAIKey;
        if (string.IsNullOrEmpty(openAIKey))
        {
            error = "Add your OpenAI API key in Settings.";
            return null;
        }
        return new OpenAITextTransformer(openAIKey, settings.OpenAITextModel);
    }
}

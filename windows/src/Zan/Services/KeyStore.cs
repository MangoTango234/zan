namespace Zan.Services;

/// <summary>
/// API keys in Windows Credential Manager. Mirrors the macOS KeychainStore: the
/// stable service label "dev.local.zan" is preserved so the concept maps 1:1.
/// Keys are never logged or written to disk in plaintext.
/// </summary>
internal static class KeyStore
{
    private const string Service = "dev.local.zan";
    private const string OpenAIAccount = "openai-api-key";
    private const string AnthropicAccount = "anthropic-api-key";

    private static string Target(string account) => $"{Service}/{account}";

    public static string? OpenAIKey => Get(OpenAIAccount);
    public static string? AnthropicKey => Get(AnthropicAccount);

    public static bool HasOpenAIKey => !string.IsNullOrEmpty(OpenAIKey);
    public static bool HasAnthropicKey => !string.IsNullOrEmpty(AnthropicKey);

    public static void SetOpenAIKey(string value) => Set(OpenAIAccount, value);
    public static void SetAnthropicKey(string value) => Set(AnthropicAccount, value);

    private static string? Get(string account)
    {
        var value = CredentialStore.Read(Target(account));
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static void Set(string account, string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            CredentialStore.Delete(Target(account));
        else
            CredentialStore.Save(Target(account), account, trimmed);
    }
}

using System.IO;
using System.Text.Json;
using Zan.Models;

namespace Zan.Services;

/// <summary>
/// Loads and persists <see cref="AppSettings"/> to %APPDATA%\Zan\settings.json.
/// The cleanup prompt defaults to the shared catalog's dictationCleanupPrompt
/// on first run.
/// </summary>
internal static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static AppSettings Load(ActionCatalog seed)
    {
        var path = AppPaths.SettingsFile();
        if (File.Exists(path))
        {
            try
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Options);
                if (settings != null)
                {
                    if (string.IsNullOrWhiteSpace(settings.CleanupPrompt))
                        settings.CleanupPrompt = seed.DictationCleanupPrompt;
                    return settings;
                }
            }
            catch (JsonException)
            {
                // Corrupt settings: fall through to defaults rather than crash.
            }
        }

        return new AppSettings { CleanupPrompt = seed.DictationCleanupPrompt };
    }

    public static void Save(AppSettings settings)
    {
        File.WriteAllText(AppPaths.SettingsFile(), JsonSerializer.Serialize(settings, Options));
    }
}

using System.IO;

namespace Zan.Services;

/// <summary>
/// Shared on-disk locations. User config lives in %APPDATA%\Zan (outside the
/// app folder) so updates never wipe actions, prompts, or settings. Mirrors the
/// macOS AppPaths (~/Library/Application Support/Zan).
/// </summary>
internal static class AppPaths
{
    public static string AppDataDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zan");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string ActionsFile() => Path.Combine(AppDataDir(), "actions.json");

    public static string SettingsFile() => Path.Combine(AppDataDir(), "settings.json");
}

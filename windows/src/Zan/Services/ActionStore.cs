using System.IO;
using System.Text.Json;
using Zan.Models;

namespace Zan.Services;

/// <summary>
/// Loads the canonical built-in catalog (shared/actions.json), which is copied
/// next to the executable at build time. Milestone 2 will layer user edits
/// persisted to %APPDATA%\Zan\ on top of these defaults.
/// </summary>
internal static class ActionStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static ActionCatalog LoadBuiltIns()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "actions.json");
        if (!File.Exists(path))
            return new ActionCatalog();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ActionCatalog>(json, Options)
                   ?? new ActionCatalog();
        }
        catch (JsonException)
        {
            // Malformed catalog: fall back to empty rather than crashing the tray.
            return new ActionCatalog();
        }
    }
}

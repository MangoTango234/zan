using System.IO;
using System.Text.Json;
using Zan.Models;

namespace Zan.Services;

/// <summary>
/// Loads the built-in catalog seed (shared/actions.json, copied next to the exe)
/// and the per-user actions file (%APPDATA%\Zan\actions.json). On first run the
/// user file is seeded from the built-ins. Mirrors the macOS ActionStore.
/// </summary>
internal static class ActionStore
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
    };

    /// Stable shortcut keys for built-ins, matched to macOS Action.defaults so
    /// hotkeys stay consistent across platforms and survive upgrades.
    private static readonly Dictionary<string, string> BuiltInShortcutKeys = new()
    {
        ["Proofread"] = "transform_proofread",
        ["Make professional"] = "transform_professional",
        ["Strip em dashes"] = "transform_stripemdash",
        ["Translate to English"] = "translatePopup",
        ["Summarize"] = "summaryPopup",
        ["Open in r.jina.ai"] = "jinaReader",
    };

    /// <summary>Reads the bundled shared catalog (cleanup prompt + built-in actions).</summary>
    public static ActionCatalog LoadSeed()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "actions.json");
        ActionCatalog? catalog = null;
        if (File.Exists(path))
        {
            try
            {
                catalog = JsonSerializer.Deserialize<ActionCatalog>(File.ReadAllText(path), ReadOptions);
            }
            catch (JsonException)
            {
                // Malformed seed: fall back to empty rather than crashing the tray.
            }
        }

        catalog ??= new ActionCatalog();
        foreach (var action in catalog.Actions)
        {
            action.IsBuiltIn = true;
            if (string.IsNullOrEmpty(action.ShortcutKey))
            {
                action.ShortcutKey = BuiltInShortcutKeys.TryGetValue(action.Name, out var key)
                    ? key
                    : NewShortcutKey();
            }
        }

        return catalog;
    }

    /// <summary>Loads the user's actions, seeding from the built-ins on first run.</summary>
    public static List<ActionItem> Load(ActionCatalog seed)
    {
        var path = AppPaths.ActionsFile();
        if (File.Exists(path))
        {
            try
            {
                var doc = JsonSerializer.Deserialize<ActionsDocument>(File.ReadAllText(path), ReadOptions);
                if (doc?.Actions is { Count: > 0 })
                    return doc.Actions;
            }
            catch (JsonException)
            {
                // Corrupt user file: re-seed rather than leaving the user with nothing.
            }
        }

        var seeded = seed.Actions.Select(a => a.Clone()).ToList();
        Save(seeded);
        return seeded;
    }

    public static void Save(List<ActionItem> actions)
    {
        var doc = new ActionsDocument { Actions = actions };
        File.WriteAllText(AppPaths.ActionsFile(), JsonSerializer.Serialize(doc, WriteOptions));
    }

    public static string NewShortcutKey() => "action_" + Guid.NewGuid().ToString("N");
}

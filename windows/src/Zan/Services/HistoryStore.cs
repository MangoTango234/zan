using System.IO;
using System.Text.Json;
using Zan.Models;

namespace Zan.Services;

/// <summary>
/// In-app history of past actions/dictations, persisted to
/// %APPDATA%\Zan\history.json. Most-recent-first, capped at <see cref="Max"/>.
/// Accessed on the UI thread.
/// </summary>
internal static class HistoryStore
{
    private const int Max = 200;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private static List<HistoryEntry>? _cache;

    public static IReadOnlyList<HistoryEntry> All()
    {
        Ensure();
        return _cache!;
    }

    public static void Add(HistoryEntry entry)
    {
        Ensure();
        _cache!.Insert(0, entry);
        if (_cache.Count > Max)
            _cache.RemoveRange(Max, _cache.Count - Max);
        Save();
    }

    public static void Clear()
    {
        Ensure();
        _cache!.Clear();
        Save();
    }

    private static void Ensure()
    {
        if (_cache != null) return;

        var path = AppPaths.HistoryFile();
        if (File.Exists(path))
        {
            try
            {
                var doc = JsonSerializer.Deserialize<HistoryDocument>(File.ReadAllText(path), Options);
                _cache = doc?.Entries ?? new List<HistoryEntry>();
                return;
            }
            catch (JsonException)
            {
                // Corrupt history is non-critical: start fresh.
            }
        }

        _cache = new List<HistoryEntry>();
    }

    private static void Save()
    {
        var doc = new HistoryDocument { Entries = _cache! };
        File.WriteAllText(AppPaths.HistoryFile(), JsonSerializer.Serialize(doc, Options));
    }
}

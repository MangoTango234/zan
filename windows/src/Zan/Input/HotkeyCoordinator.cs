using Zan.Models;

namespace Zan.Input;

/// <summary>
/// Maps each action's hotkey and the dictation hotkey to a handler, (re)registering
/// them as the catalog or settings change. Combos that fail to register (already
/// taken) are reported via the conflicts callback so the user can pick another.
/// </summary>
internal sealed class HotkeyCoordinator : IDisposable
{
    private readonly HotkeyService _service = new();
    private readonly Action<ActionItem> _onAction;
    private readonly Action _onDictation;
    private readonly Action<IReadOnlyList<string>> _onConflicts;

    public HotkeyCoordinator(
        Action<ActionItem> onAction,
        Action onDictation,
        Action<IReadOnlyList<string>> onConflicts)
    {
        _onAction = onAction;
        _onDictation = onDictation;
        _onConflicts = onConflicts;
    }

    public void Rebind(IEnumerable<ActionItem> actions, AppSettings settings)
    {
        _service.UnregisterAll();
        var conflicts = new List<string>();

        foreach (var action in actions)
        {
            if (!HotkeyCombo.TryParse(action.Hotkey, out var combo))
                continue;

            var captured = action; // avoid closure over the loop variable
            if (_service.Register(combo, () => _onAction(captured)) < 0)
                conflicts.Add($"{action.Name} ({action.Hotkey})");
        }

        if (HotkeyCombo.TryParse(settings.DictationHotkey, out var dictationCombo))
        {
            if (_service.Register(dictationCombo, _onDictation) < 0)
                conflicts.Add($"Dictation ({settings.DictationHotkey})");
        }

        if (conflicts.Count > 0)
            _onConflicts(conflicts);
    }

    public void Dispose() => _service.Dispose();
}

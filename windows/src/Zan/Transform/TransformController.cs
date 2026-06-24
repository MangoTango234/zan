using Zan.Injection;
using Zan.Models;
using Zan.Services;

namespace Zan.Transform;

/// <summary>
/// Runs a triggered action end to end: read the selection, run the engine
/// (LLM prompt or fixed prefix), then deliver per output mode (replace the
/// selection, show a popup, or copy). Mirrors the macOS TransformController.
/// Single-flight: a new trigger is ignored while one is in progress.
/// </summary>
internal sealed class TransformController
{
    private readonly AppSettings _settings;
    private readonly ITransformUi _ui;
    private bool _busy;

    public TransformController(AppSettings settings, ITransformUi ui)
    {
        _settings = settings;
        _ui = ui;
    }

    /// <summary>Fire-and-forget entry point for the hotkey handler (runs on the UI thread).</summary>
    public async void Run(ActionItem action)
    {
        if (_busy) return;
        _busy = true;
        try
        {
            await RunAsync(action);
        }
        catch (Exception ex)
        {
            _ui.HideWorking();
            _ui.ShowResult("Zan: error", ex.Message);
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunAsync(ActionItem action)
    {
        var selection = await SelectionReader.ReadSelectionAsync();
        if (string.IsNullOrWhiteSpace(selection))
        {
            _ui.Notify("No text selected.");
            return;
        }

        string result;
        if (action.Engine == "prefix")
        {
            result = (action.Prefix ?? string.Empty) + selection;
        }
        else
        {
            var transformer = TextEngineFactory.Create(_settings, out var error);
            if (transformer == null)
            {
                _ui.ShowResult("Zan", error);
                return;
            }

            _ui.ShowWorking(action.Name);
            try
            {
                result = await transformer.TransformAsync(action.Prompt ?? string.Empty, selection, CancellationToken.None);
            }
            finally
            {
                _ui.HideWorking();
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            _ui.Notify("Empty result.");
            return;
        }

        HistoryStore.Add(new HistoryEntry
        {
            Kind = "action",
            Title = action.Name,
            Input = selection,
            Output = result,
        });

        switch (action.Output)
        {
            case "popup":
                _ui.ShowResult(action.Name, result);
                break;
            case "copy":
                ClipboardHelper.SetText(result);
                _ui.Notify("Copied to clipboard.");
                break;
            default: // replaceSelection
                await TextInjector.PasteAsync(result);
                break;
        }
    }
}

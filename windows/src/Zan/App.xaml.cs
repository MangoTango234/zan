using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Zan.Input;
using Zan.Models;
using Zan.Services;
using Zan.Views;

namespace Zan;

/// <summary>
/// Application entry point. Zan is a tray-only utility: at startup it loads the
/// catalog seed, user settings, and user actions, installs the tray icon, and
/// registers global hotkeys. The Settings window is opened on demand (and on
/// first run, when no API key is configured yet).
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _tray;
    private SettingsWindow? _settingsWindow;
    private HotkeyCoordinator? _hotkeys;

    private ActionCatalog _seed = new();
    private AppSettings _settings = new();
    private List<ActionItem> _actions = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _seed = ActionStore.LoadSeed();
        _settings = SettingsStore.Load(_seed);
        _actions = ActionStore.Load(_seed);

        _tray = TrayIconFactory.Create(OpenSettings, Quit);

        _hotkeys = new HotkeyCoordinator(OnActionHotkey, OnDictationHotkey, OnHotkeyConflicts);
        _hotkeys.Rebind(_actions, _settings);

        // First-run guidance: if nothing is configured yet, the app would be an
        // invisible tray icon. Open Settings so the user can add a key and hotkeys.
        if (!KeyStore.HasOpenAIKey && !KeyStore.HasAnthropicKey)
            OpenSettings();
    }

    private void OpenSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_seed, _settings, _actions, RebindHotkeys);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            if (_settingsWindow.WindowState == WindowState.Minimized)
                _settingsWindow.WindowState = WindowState.Normal;
            _settingsWindow.Activate();
        }
    }

    private void RebindHotkeys() => _hotkeys?.Rebind(_actions, _settings);

    // MARK: - Hotkey handlers
    //
    // Milestone 3 wires the hotkeys end to end with placeholder feedback. The
    // real behavior arrives later: triggering an action (read selection -> run ->
    // deliver) in milestone 4, and dictation (record -> transcribe -> insert) in
    // milestone 5.

    private void OnActionHotkey(ActionItem action)
    {
        _tray?.ShowBalloonTip("Zan", $"Action triggered: {action.Name}", BalloonIcon.Info);
    }

    private void OnDictationHotkey()
    {
        _tray?.ShowBalloonTip("Zan", "Dictation triggered", BalloonIcon.Info);
    }

    private void OnHotkeyConflicts(IReadOnlyList<string> conflicts)
    {
        _tray?.ShowBalloonTip(
            "Zan: hotkey not registered",
            "Already in use, pick another:\n" + string.Join("\n", conflicts),
            BalloonIcon.Warning);
    }

    private void Quit()
    {
        _hotkeys?.Dispose();
        _hotkeys = null;
        _tray?.Dispose();
        _tray = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeys?.Dispose();
        _hotkeys = null;
        _tray?.Dispose();
        _tray = null;
        base.OnExit(e);
    }
}

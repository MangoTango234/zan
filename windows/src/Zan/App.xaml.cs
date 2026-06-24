using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Zan.Input;
using Zan.Models;
using Zan.Services;
using Zan.Transform;
using Zan.Views;

namespace Zan;

/// <summary>
/// Application entry point. Zan is a tray-only utility: at startup it loads the
/// catalog seed, user settings, and user actions, installs the tray icon, and
/// registers global hotkeys. Action hotkeys run end to end (read selection -> run
/// -> deliver); the Settings window opens on demand (and on first run).
/// </summary>
public partial class App : Application, ITransformUi
{
    private TaskbarIcon? _tray;
    private SettingsWindow? _settingsWindow;
    private HotkeyCoordinator? _hotkeys;
    private TransformController? _transform;
    private TransformHud? _hud;

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
        _transform = new TransformController(_settings, this);

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

    private void OnActionHotkey(ActionItem action) => _transform?.Run(action);

    private void OnDictationHotkey()
    {
        // Dictation (record -> transcribe -> insert) arrives in milestone 5.
        _tray?.ShowBalloonTip("Zan", "Dictation triggered", BalloonIcon.Info);
    }

    private void OnHotkeyConflicts(IReadOnlyList<string> conflicts)
    {
        _tray?.ShowBalloonTip(
            "Zan: hotkey not registered",
            "Already in use, pick another:\n" + string.Join("\n", conflicts),
            BalloonIcon.Warning);
    }

    // MARK: - ITransformUi

    public void ShowWorking(string title)
    {
        _hud?.Close();
        _hud = new TransformHud(title);
        _hud.Show(); // ShowActivated=False, so this does not steal focus from the target app
    }

    public void HideWorking()
    {
        _hud?.Close();
        _hud = null;
    }

    public void ShowResult(string title, string body)
    {
        new PopupWindow(title, body).Show();
    }

    public void Notify(string message)
    {
        _tray?.ShowBalloonTip("Zan", message, BalloonIcon.Info);
    }

    // MARK: - Lifecycle

    private void Quit()
    {
        _hud?.Close();
        _hotkeys?.Dispose();
        _hotkeys = null;
        _tray?.Dispose();
        _tray = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hud?.Close();
        _hotkeys?.Dispose();
        _hotkeys = null;
        _tray?.Dispose();
        _tray = null;
        base.OnExit(e);
    }
}

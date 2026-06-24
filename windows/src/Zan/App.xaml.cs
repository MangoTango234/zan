using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Zan.Models;
using Zan.Services;
using Zan.Views;

namespace Zan;

/// <summary>
/// Application entry point. Zan is a tray-only utility: at startup it loads the
/// catalog seed, user settings, and user actions, then installs the tray icon and
/// lives in the notification area until the user picks Quit. The Settings window
/// is opened on demand (and on first run, when no API key is configured yet).
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _tray;
    private SettingsWindow? _settingsWindow;

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

        // First-run guidance: if nothing is configured yet, the app would be an
        // invisible tray icon. Open Settings so the user can add a key.
        if (!KeyStore.HasOpenAIKey && !KeyStore.HasAnthropicKey)
            OpenSettings();
    }

    private void OpenSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_seed, _settings, _actions);
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

    private void Quit()
    {
        _tray?.Dispose();
        _tray = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _tray = null;
        base.OnExit(e);
    }
}

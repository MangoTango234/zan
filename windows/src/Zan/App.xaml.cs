using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Zan.Services;

namespace Zan;

/// <summary>
/// Application entry point. Zan is a tray-only utility: at startup it loads the
/// built-in action catalog and installs the tray icon, then lives in the
/// notification area until the user picks Quit.
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var catalog = ActionStore.LoadBuiltIns();
        _tray = TrayIconFactory.Create(catalog, Quit);
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

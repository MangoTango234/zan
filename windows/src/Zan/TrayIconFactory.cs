using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace Zan;

/// <summary>
/// Builds the tray icon and its right-click menu. Later milestones add per-action
/// triggers and dictation state; for now it opens Settings and quits.
/// </summary>
internal static class TrayIconFactory
{
    public static TaskbarIcon Create(Action onSettings, Action onQuit)
    {
        var tray = new TaskbarIcon
        {
            ToolTipText = "Zan",
            IconSource = new BitmapImage(
                new Uri("pack://application:,,,/Assets/zan.png", UriKind.Absolute)),
            ContextMenu = BuildMenu(onSettings, onQuit),
        };

        // Left-click (and double-click) also opens Settings, the usual tray idiom.
        tray.TrayLeftMouseUp += (_, _) => onSettings();

        return tray;
    }

    private static ContextMenu BuildMenu(Action onSettings, Action onQuit)
    {
        var menu = new ContextMenu();

        menu.Items.Add(new MenuItem { Header = "Zan", IsEnabled = false });
        menu.Items.Add(new Separator());

        var settings = new MenuItem { Header = "Settings…" };
        settings.Click += (_, _) => onSettings();
        menu.Items.Add(settings);

        menu.Items.Add(new Separator());

        var quit = new MenuItem { Header = "Quit Zan" };
        quit.Click += (_, _) => onQuit();
        menu.Items.Add(quit);

        return menu;
    }
}

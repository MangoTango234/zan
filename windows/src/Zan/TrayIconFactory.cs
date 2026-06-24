using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Zan.Models;

namespace Zan;

/// <summary>
/// Builds the tray icon and its right-click menu. For milestone 1 the menu just
/// proves the app is alive and that the shared catalog loaded; later milestones
/// add Settings, per-action triggers, and dictation.
/// </summary>
internal static class TrayIconFactory
{
    public static TaskbarIcon Create(ActionCatalog catalog, Action onQuit)
    {
        var tray = new TaskbarIcon
        {
            ToolTipText = "Zan",
            IconSource = new BitmapImage(
                new Uri("pack://application:,,,/Assets/zan.png", UriKind.Absolute)),
            ContextMenu = BuildMenu(catalog, onQuit),
        };

        return tray;
    }

    private static ContextMenu BuildMenu(ActionCatalog catalog, Action onQuit)
    {
        var menu = new ContextMenu();

        menu.Items.Add(new MenuItem { Header = "Zan", IsEnabled = false });
        menu.Items.Add(new MenuItem
        {
            Header = $"{catalog.Actions.Count} built-in actions loaded",
            IsEnabled = false,
        });

        menu.Items.Add(new Separator());

        var quit = new MenuItem { Header = "Quit Zan" };
        quit.Click += (_, _) => onQuit();
        menu.Items.Add(quit);

        return menu;
    }
}

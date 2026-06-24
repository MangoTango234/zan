using System.Windows;

namespace Zan.Views;

/// <summary>
/// A small, non-activating "working" HUD shown during the LLM call. It must not
/// steal focus (ShowActivated=False) so the synthetic paste lands in the app the
/// user triggered from, not in our window.
/// </summary>
public partial class TransformHud : Window
{
    public TransformHud(string title)
    {
        InitializeComponent();
        Label.Text = $"Zan: {title}…";
    }
}

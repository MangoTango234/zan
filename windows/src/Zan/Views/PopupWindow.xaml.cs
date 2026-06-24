using System.Windows;
using Zan.Injection;

namespace Zan.Views;

/// <summary>
/// Read-only result window for "popup" actions (and errors). The text is
/// selectable and copyable; Esc/Close dismisses it.
/// </summary>
public partial class PopupWindow : Window
{
    private readonly string _body;

    public PopupWindow(string header, string body)
    {
        InitializeComponent();
        _body = body;
        HeaderText.Text = header;
        BodyText.Text = body;
    }

    private void Copy_Click(object sender, RoutedEventArgs e) => ClipboardHelper.SetText(_body);

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Zan.Models;
using Zan.Services;

namespace Zan.Views;

/// <summary>
/// The settings surface: API keys (Credential Manager), provider/model pickers,
/// dictation cleanup, and the editable actions list. Changes persist to
/// %APPDATA%\Zan via the stores. The macOS app spreads these across several
/// section views; here they are tabs in one window.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly ActionCatalog _seed;
    private readonly AppSettings _settings;
    private readonly List<ActionItem> _sharedActions;
    private readonly ObservableCollection<ActionItem> _actions;
    private readonly Action _onHotkeysChanged;

    private bool _loading;

    private const string StarterPrompt =
        "Rewrite the selected text to [describe the change you want]. Keep the meaning. Return only the result.";

    public SettingsWindow(ActionCatalog seed, AppSettings settings, List<ActionItem> actions, Action onHotkeysChanged)
    {
        InitializeComponent();
        _seed = seed;
        _settings = settings;
        _sharedActions = actions;
        _actions = new ObservableCollection<ActionItem>(actions.Select(a => a.Clone()));
        _onHotkeysChanged = onHotkeysChanged;

        _loading = true;
        LoadKeysTab();
        LoadProvidersTab();
        LoadDictationTab();
        LoadActionsTab();
        _loading = false;
    }

    // MARK: - API Keys

    private void LoadKeysTab()
    {
        OpenAIStatus.Text = KeyStore.HasOpenAIKey ? "A key is stored." : "No key stored.";
        AnthropicStatus.Text = KeyStore.HasAnthropicKey ? "A key is stored." : "No key stored.";
    }

    private void SaveOpenAIKey_Click(object sender, RoutedEventArgs e)
    {
        KeyStore.SetOpenAIKey(OpenAIKeyBox.Password);
        OpenAIKeyBox.Clear();
        OpenAIStatus.Text = KeyStore.HasOpenAIKey ? "Saved." : "No key stored.";
    }

    private void ClearOpenAIKey_Click(object sender, RoutedEventArgs e)
    {
        KeyStore.SetOpenAIKey(string.Empty);
        OpenAIKeyBox.Clear();
        OpenAIStatus.Text = "No key stored.";
    }

    private void SaveAnthropicKey_Click(object sender, RoutedEventArgs e)
    {
        KeyStore.SetAnthropicKey(AnthropicKeyBox.Password);
        AnthropicKeyBox.Clear();
        AnthropicStatus.Text = KeyStore.HasAnthropicKey ? "Saved." : "No key stored.";
    }

    private void ClearAnthropicKey_Click(object sender, RoutedEventArgs e)
    {
        KeyStore.SetAnthropicKey(string.Empty);
        AnthropicKeyBox.Clear();
        AnthropicStatus.Text = "No key stored.";
    }

    // MARK: - Providers & Models

    private void LoadProvidersTab()
    {
        TextProviderCombo.ItemsSource = new[] { "OpenAI", "Anthropic" };
        TextProviderCombo.SelectedIndex = _settings.TextProvider == "anthropic" ? 1 : 0;
        ApplyTextProviderToModelCombo();

        TranscriptionProviderCombo.ItemsSource = new[] { "OpenAI (cloud)", "On-device" };
        TranscriptionProviderCombo.SelectedIndex = _settings.TranscriptionProvider == "local" ? 1 : 0;
        ApplyTranscriptionProviderToModelCombo();
    }

    private void ApplyTextProviderToModelCombo()
    {
        var openai = TextProviderCombo.SelectedIndex == 0;
        TextModelCombo.ItemsSource = openai ? AppSettings.OpenAITextModels : AppSettings.AnthropicTextModels;
        TextModelCombo.Text = openai ? _settings.OpenAITextModel : _settings.AnthropicTextModel;
    }

    private void ApplyTranscriptionProviderToModelCombo()
    {
        var openai = TranscriptionProviderCombo.SelectedIndex == 0;
        TranscriptionModelCombo.ItemsSource = openai ? AppSettings.TranscriptionModels : AppSettings.WhisperModels;
        TranscriptionModelCombo.Text = openai ? _settings.TranscriptionModel : _settings.WhisperModel;
    }

    private void TextProvider_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        ApplyTextProviderToModelCombo();
    }

    private void TranscriptionProvider_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        ApplyTranscriptionProviderToModelCombo();
    }

    private void SaveProviders_Click(object sender, RoutedEventArgs e)
    {
        var textOpenai = TextProviderCombo.SelectedIndex == 0;
        _settings.TextProvider = textOpenai ? "openai" : "anthropic";
        var textModel = TextModelCombo.Text.Trim();
        if (textModel.Length > 0)
        {
            if (textOpenai) _settings.OpenAITextModel = textModel;
            else _settings.AnthropicTextModel = textModel;
        }

        var transOpenai = TranscriptionProviderCombo.SelectedIndex == 0;
        _settings.TranscriptionProvider = transOpenai ? "openai" : "local";
        var transModel = TranscriptionModelCombo.Text.Trim();
        if (transModel.Length > 0)
        {
            if (transOpenai) _settings.TranscriptionModel = transModel;
            else _settings.WhisperModel = transModel;
        }

        SettingsStore.Save(_settings);
        ProvidersStatus.Text = "Saved";
    }

    // MARK: - Dictation

    private void LoadDictationTab()
    {
        CleanupEnabledCheck.IsChecked = _settings.CleanupEnabled;
        DictationModeCombo.ItemsSource = new[] { "Toggle (press to start/stop)", "Hold to talk" };
        DictationModeCombo.SelectedIndex = _settings.DictationMode == "holdToTalk" ? 1 : 0;
        DictationHotkeyRecorder.Hotkey = _settings.DictationHotkey;
        CleanupPromptBox.Text = _settings.CleanupPrompt;
    }

    private void SaveDictation_Click(object sender, RoutedEventArgs e)
    {
        _settings.CleanupEnabled = CleanupEnabledCheck.IsChecked == true;
        _settings.DictationMode = DictationModeCombo.SelectedIndex == 1 ? "holdToTalk" : "toggle";
        _settings.DictationHotkey = DictationHotkeyRecorder.Hotkey;
        _settings.CleanupPrompt = CleanupPromptBox.Text;
        SettingsStore.Save(_settings);
        _onHotkeysChanged();
        DictationStatus.Text = "Saved";
    }

    // MARK: - Actions

    private void LoadActionsTab()
    {
        ActionEngine.ItemsSource = new[] { "AI prompt", "Prefix (no AI)" };
        ActionOutput.ItemsSource = new[] { "Replace selection", "Show popup", "Copy to clipboard" };
        ActionsList.ItemsSource = _actions;
        if (_actions.Count > 0)
            ActionsList.SelectedIndex = 0;
    }

    private ActionItem? Selected => ActionsList.SelectedItem as ActionItem;

    private void ActionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = Selected;
        ActionDetailPanel.IsEnabled = item != null;
        if (item == null) return;

        _loading = true;
        ActionName.Text = item.Name;
        ActionDetail.Text = item.Detail;
        ActionEngine.SelectedIndex = item.Engine == "prefix" ? 1 : 0;
        ActionOutput.SelectedIndex = item.Output switch
        {
            "popup" => 1,
            "copy" => 2,
            _ => 0,
        };
        ActionPrompt.Text = item.Prompt ?? string.Empty;
        ActionPrefix.Text = item.Prefix ?? string.Empty;
        ActionHotkeyRecorder.Hotkey = item.Hotkey;
        UpdateEngineFieldVisibility();
        _loading = false;
    }

    private void ActionEngine_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        UpdateEngineFieldVisibility();
    }

    private void UpdateEngineFieldVisibility()
    {
        var prefixEngine = ActionEngine.SelectedIndex == 1;
        PromptLabel.Visibility = prefixEngine ? Visibility.Collapsed : Visibility.Visible;
        ActionPrompt.Visibility = prefixEngine ? Visibility.Collapsed : Visibility.Visible;
        PrefixLabel.Visibility = prefixEngine ? Visibility.Visible : Visibility.Collapsed;
        ActionPrefix.Visibility = prefixEngine ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyAction_Click(object sender, RoutedEventArgs e)
    {
        var item = Selected;
        if (item == null) return;

        item.Name = string.IsNullOrWhiteSpace(ActionName.Text) ? "Untitled" : ActionName.Text.Trim();
        item.Detail = ActionDetail.Text.Trim();
        item.Engine = ActionEngine.SelectedIndex == 1 ? "prefix" : "ai";
        item.Output = ActionOutput.SelectedIndex switch
        {
            1 => "popup",
            2 => "copy",
            _ => "replaceSelection",
        };
        item.Prompt = ActionPrompt.Text;
        item.Prefix = ActionPrefix.Text;
        item.Hotkey = ActionHotkeyRecorder.Hotkey;

        ActionsList.Items.Refresh();
        ActionsStatus.Text = "Applied (not yet saved)";
    }

    private void AddAction_Click(object sender, RoutedEventArgs e)
    {
        var item = new ActionItem
        {
            Name = "New action",
            Detail = "",
            Engine = "ai",
            Output = "replaceSelection",
            Prompt = StarterPrompt,
            Prefix = "",
            ShortcutKey = ActionStore.NewShortcutKey(),
            IsBuiltIn = false,
        };
        _actions.Add(item);
        ActionsList.SelectedItem = item;
        ActionName.Focus();
    }

    private void DeleteAction_Click(object sender, RoutedEventArgs e)
    {
        var item = Selected;
        if (item == null) return;
        var index = _actions.IndexOf(item);
        _actions.Remove(item);
        if (_actions.Count > 0)
            ActionsList.SelectedIndex = Math.Min(index, _actions.Count - 1);
        else
            ActionDetailPanel.IsEnabled = false;
    }

    private void SaveActions_Click(object sender, RoutedEventArgs e)
    {
        var list = _actions.ToList();
        ActionStore.Save(list);

        // Keep the app's in-memory list in sync with what we just persisted.
        _sharedActions.Clear();
        _sharedActions.AddRange(list);

        _onHotkeysChanged();
        ActionsStatus.Text = "Saved";
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DisableWin11Search.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DisableWin11Search;

public partial class MainWindow : FluentWindow
{
    private readonly RegistryService _registryService = new();
    private readonly LocalizationService _localizationService = new();
    private readonly ThemePreferenceService _themePreferenceService = new();

    private static readonly Color LightMicaTintColor = Color.FromArgb(0x40, 0xff, 0xff, 0xff);
    private static readonly Color LightFooterColor = Color.FromArgb(0x99, 0xff, 0xff, 0xff);
    private static readonly Color LightDialogColor = Color.FromArgb(0xf2, 0xff, 0xff, 0xff);
    private static readonly Color DarkMicaTintColor = Color.FromArgb(0x40, 0x20, 0x20, 0x20);
    private static readonly Color DarkFooterColor = Color.FromArgb(0x99, 0x1f, 0x1f, 0x1f);
    private static readonly Color DarkDialogColor = Color.FromArgb(0xf2, 0x2b, 0x2b, 0x2b);
    private static readonly Color SuccessColor = Color.FromRgb(0x0f, 0x7b, 0x0f);
    private static readonly Color DangerColor = Color.FromRgb(0xc4, 0x2b, 0x1c);
    private static readonly Color DarkSuccessColor = Color.FromRgb(0x6c, 0xd6, 0x6c);
    private static readonly Color DarkDangerColor = Color.FromRgb(0xff, 0x8a, 0x80);

    public MainWindow()
    {
        InitializeComponent();

        ApplicationThemeManager.Changed += OnApplicationThemeChanged;
        _themePreferenceService.Apply(this, _themePreferenceService.CurrentPreference, save: false);
        UpdateThemeResources(ApplicationThemeManager.GetAppTheme());

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyLocalization();
        RefreshStatus();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        ApplicationThemeManager.Changed -= OnApplicationThemeChanged;
        SystemThemeWatcher.UnWatch(this);
    }

    private void OnApplicationThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        UpdateThemeResources(currentApplicationTheme);
        RefreshStatus();
    }

    private void UpdateThemeResources(ApplicationTheme applicationTheme)
    {
        var isDark = applicationTheme == ApplicationTheme.Dark;

        SetBrushResource("AppMicaTintBrush", isDark ? DarkMicaTintColor : LightMicaTintColor);
        SetBrushResource("AppFooterBrush", isDark ? DarkFooterColor : LightFooterColor);
        SetBrushResource("AppDialogBrush", isDark ? DarkDialogColor : LightDialogColor);
        SetBrushResource("AppSuccessBrush", isDark ? DarkSuccessColor : SuccessColor);
        SetBrushResource("AppDangerBrush", isDark ? DarkDangerColor : DangerColor);
    }

    private void SetBrushResource(string resourceKey, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        Resources[resourceKey] = brush;
    }

    private void ApplyLocalization()
    {
        Title = T("AppTitle");
        AppTitleBar.Title = string.Empty;
        HeaderTitleText.Text = T("AppTitle");
        HeaderSubtitleText.Text = T("AppSubtitle");

        SuggestionsTitleText.Text = T("SuggestionsTitle");
        SuggestionsDescriptionText.Text = T("SuggestionsDescription");
        CloudTitleText.Text = T("CloudTitle");
        CloudDescriptionText.Text = T("CloudDescription");
        BingTitleText.Text = T("BingTitle");
        BingDescriptionText.Text = T("BingDescription");
        WebResultsTitleText.Text = T("WebResultsTitle");
        WebResultsDescriptionText.Text = T("WebResultsDescription");

        SetActionButtonText(ApplySuggestionsButton, RevertSuggestionsButton);
        SetActionButtonText(ApplyCloudButton, RevertCloudButton);
        SetActionButtonText(ApplyBingButton, RevertBingButton);
        SetActionButtonText(ApplyWebResultsButton, RevertWebResultsButton);

        CreditsText.Text = T("Credits");
        RestartExplorerButton.Content = T("RestartExplorer");
        RestartExplorerButton.ToolTip = T("RestartExplorerTooltip");
        SettingsButton.ToolTip = T("SettingsTooltip");

        SettingsDialogTitleText.Text = T("SettingsDialogTitle");
        SettingsDialogDescriptionText.Text = T("SettingsDialogDescription");
        LanguageSectionTitleText.Text = T("LanguageSectionTitle");
        ThemeSectionTitleText.Text = T("ThemeSectionTitle");
        PortugueseButton.Content = T("PortugueseBrazil");
        EnglishButton.Content = T("English");
        SystemThemeButton.Content = T("SystemTheme");
        LightThemeButton.Content = T("LightTheme");
        DarkThemeButton.Content = T("DarkTheme");
        CloseSettingsButton.Content = T("Close");

        RestartConfirmationTitleText.Text = T("RestartExplorerConfirmTitle");
        RestartConfirmationMessageText.Text = T("RestartExplorerConfirmMessage");
        CancelRestartExplorerButton.Content = T("Cancel");
        ConfirmRestartExplorerButton.Content = T("RestartExplorer");

        RefreshLanguageSelection();
        RefreshThemeSelection();
        RefreshStatus();
    }

    private void SetActionButtonText(System.Windows.Controls.Button applyButton, System.Windows.Controls.Button revertButton)
    {
        applyButton.Content = T("Apply");
        applyButton.ToolTip = T("ApplyTooltip");
        revertButton.Content = T("Revert");
        revertButton.ToolTip = T("RevertTooltip");
    }

    private void RefreshLanguageSelection()
    {
        var isPortuguese = _localizationService.CurrentCulture.Name.Equals("pt-BR", StringComparison.OrdinalIgnoreCase);
        PortugueseButton.Appearance = isPortuguese ? ControlAppearance.Primary : ControlAppearance.Secondary;
        EnglishButton.Appearance = isPortuguese ? ControlAppearance.Secondary : ControlAppearance.Primary;
    }

    private void RefreshThemeSelection()
    {
        SystemThemeButton.Appearance = _themePreferenceService.CurrentPreference == ThemePreference.System ? ControlAppearance.Primary : ControlAppearance.Secondary;
        LightThemeButton.Appearance = _themePreferenceService.CurrentPreference == ThemePreference.Light ? ControlAppearance.Primary : ControlAppearance.Secondary;
        DarkThemeButton.Appearance = _themePreferenceService.CurrentPreference == ThemePreference.Dark ? ControlAppearance.Primary : ControlAppearance.Secondary;
    }

    private void RefreshStatus()
    {
        UpdateStatusUI(IconSuggestions, TextSuggestions, _registryService.CheckSearchBoxSuggestions());
        UpdateStatusUI(IconCloud, TextCloud, _registryService.CheckCloudSearch());
        UpdateStatusUI(IconBing, TextBing, _registryService.CheckBingSearch());
        UpdateStatusUI(IconWebResults, TextWebResults, _registryService.CheckWebResults());
    }

    private void UpdateStatusUI(SymbolIcon icon, System.Windows.Controls.TextBlock textBlock, RegistryService.OptimizationStatus status)
    {
        (textBlock.Text, icon.Symbol, icon.Foreground) = status switch
        {
            RegistryService.OptimizationStatus.Optimized =>
                (T("Optimized"), SymbolRegular.CheckmarkCircle24, FindBrush("AppSuccessBrush")),

            RegistryService.OptimizationStatus.NotOptimized =>
                (T("NotOptimized"), SymbolRegular.DismissCircle24, FindBrush("AppDangerBrush")),

            _ =>
                (T("Unknown"), SymbolRegular.QuestionCircle24, Brushes.Gray)
        };
    }

    private Brush FindBrush(string resourceKey)
    {
        return TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    private void ApplySuggestions_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.ApplySearchBoxSuggestions(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RevertSuggestions_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.RevertSearchBoxSuggestions(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ApplyCloud_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.ApplyCloudSearch(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RevertCloud_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.RevertCloudSearch(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ApplyBing_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.ApplyBingSearch(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RevertBing_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.RevertBingSearch(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ApplyWebResults_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.ApplyWebResults(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RevertWebResults_Click(object sender, RoutedEventArgs e)
    {
        try { _registryService.RevertWebResults(); RefreshStatus(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RestartExplorer_Click(object sender, RoutedEventArgs e)
    {
        RestartConfirmationOverlay.Visibility = Visibility.Visible;
    }

    private void ConfirmRestartExplorer_Click(object sender, RoutedEventArgs e)
    {
        RestartConfirmationOverlay.Visibility = Visibility.Collapsed;

        try
        {
            _registryService.RestartExplorer();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void CancelRestartExplorer_Click(object sender, RoutedEventArgs e)
    {
        RestartConfirmationOverlay.Visibility = Visibility.Collapsed;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Visible;
    }

    private void PortugueseButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeLanguage("pt-BR");
    }

    private void EnglishButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeLanguage("en-US");
    }

    private void SystemThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(ThemePreference.System);
    }

    private void LightThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(ThemePreference.Light);
    }

    private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(ThemePreference.Dark);
    }

    private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void SettingsOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void RestartConfirmationOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        RestartConfirmationOverlay.Visibility = Visibility.Collapsed;
    }

    private void DialogPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ChangeLanguage(string cultureName)
    {
        _localizationService.ApplyCulture(cultureName);
        ApplyLocalization();
    }

    private void ChangeTheme(ThemePreference preference)
    {
        _themePreferenceService.Apply(this, preference);
        UpdateThemeResources(ApplicationThemeManager.GetAppTheme());
        RefreshThemeSelection();
        RefreshStatus();
    }

    private string T(string key) => _localizationService.GetString(key);

    private void ShowError(Exception ex)
    {
        System.Windows.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, T("ErrorMessage"), ex.Message), T("ErrorTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }
}

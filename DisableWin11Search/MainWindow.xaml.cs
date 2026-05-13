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
    private readonly ThemeService _themeService = new();


    public MainWindow()
    {
        InitializeComponent();

        _themeService.ThemeApplied += ThemeService_ThemeApplied;
        _themeService.Apply(this);

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void ThemeService_ThemeApplied(object? sender, EventArgs e)
    {
        if (IsLoaded)
        {
            RefreshStatus();
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        ApplyLocalization();
        RefreshStatus();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Closed -= MainWindow_Closed;
        _themeService.ThemeApplied -= ThemeService_ThemeApplied;
        _themeService.Dispose();
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
        ThemeSystemButton.Content = T("ThemeSystem");
        ThemeLightButton.Content = T("ThemeLight");
        ThemeDarkButton.Content = T("ThemeDark");
        CloseSettingsButton.Content = T("Close");

        RestartExplorerConfirmTitleText.Text = T("RestartExplorerConfirmTitle");
        RestartExplorerConfirmMessageText.Text = T("RestartExplorerConfirmMessage");
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
        UpdateSelectionButton(PortugueseButton, isPortuguese);
        UpdateSelectionButton(EnglishButton, !isPortuguese);
    }

    private void RefreshThemeSelection()
    {
        UpdateSelectionButton(ThemeSystemButton, _themeService.CurrentPreference == AppThemePreference.System);
        UpdateSelectionButton(ThemeLightButton, _themeService.CurrentPreference == AppThemePreference.Light);
        UpdateSelectionButton(ThemeDarkButton, _themeService.CurrentPreference == AppThemePreference.Dark);
    }

    private static void UpdateSelectionButton(Wpf.Ui.Controls.Button button, bool selected)
    {
        button.Appearance = selected ? ControlAppearance.Primary : ControlAppearance.Secondary;
        button.Icon = new SymbolIcon
        {
            Symbol = selected ? SymbolRegular.CheckmarkCircle20 : SymbolRegular.Circle20
        };
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
                (T("Optimized"), SymbolRegular.CheckmarkCircle24, GetThemeBrush("StatusSuccessBrush")),

            RegistryService.OptimizationStatus.NotOptimized =>
                (T("NotOptimized"), SymbolRegular.DismissCircle24, GetThemeBrush("StatusDangerBrush")),

            _ =>
                (T("Unknown"), SymbolRegular.QuestionCircle24, Brushes.Gray)
        };
    }

    private Brush GetThemeBrush(string key)
    {
        return TryFindResource(key) as Brush ?? Brushes.Gray;
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
        RestartExplorerOverlay.Visibility = Visibility.Visible;
    }

    private void ConfirmRestartExplorerButton_Click(object sender, RoutedEventArgs e)
    {
        RestartExplorerOverlay.Visibility = Visibility.Collapsed;

        try
        {
            _registryService.RestartExplorer();
            System.Windows.MessageBox.Show(T("RestartExplorerSuccessMessage"), T("RestartExplorerSuccessTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void CancelRestartExplorerButton_Click(object sender, RoutedEventArgs e)
    {
        RestartExplorerOverlay.Visibility = Visibility.Collapsed;
    }

    private void RestartExplorerOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        RestartExplorerOverlay.Visibility = Visibility.Collapsed;
    }

    private void RestartExplorerPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
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

    private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void SettingsOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void SettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ChangeLanguage(string cultureName)
    {
        _localizationService.ApplyCulture(cultureName);
        ApplyLocalization();
    }

    private void ThemeSystemButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(AppThemePreference.System);
    }

    private void ThemeLightButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(AppThemePreference.Light);
    }

    private void ThemeDarkButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeTheme(AppThemePreference.Dark);
    }

    private void ChangeTheme(AppThemePreference preference)
    {
        _themeService.ChangeTheme(preference, this);
        RefreshThemeSelection();
        RefreshStatus();
    }

    private string T(string key) => _localizationService.GetString(key);

    private void ShowError(Exception ex)
    {
        System.Windows.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, T("ErrorMessage"), ex.Message), T("ErrorTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }
}

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
        ApplyLocalization();
        RefreshStatus();
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
        PortugueseButton.Appearance = isPortuguese ? ControlAppearance.Primary : ControlAppearance.Secondary;
        EnglishButton.Appearance = isPortuguese ? ControlAppearance.Secondary : ControlAppearance.Primary;
    }

    private void RefreshThemeSelection()
    {
        ThemeSystemButton.Appearance = _themeService.CurrentPreference == AppThemePreference.System ? ControlAppearance.Primary : ControlAppearance.Secondary;
        ThemeLightButton.Appearance = _themeService.CurrentPreference == AppThemePreference.Light ? ControlAppearance.Primary : ControlAppearance.Secondary;
        ThemeDarkButton.Appearance = _themeService.CurrentPreference == AppThemePreference.Dark ? ControlAppearance.Primary : ControlAppearance.Secondary;
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

    private void ApplySuggestions_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.ApplySearchBoxSuggestions);

    private void RevertSuggestions_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.RevertSearchBoxSuggestions);

    private void ApplyCloud_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.ApplyCloudSearch);

    private void RevertCloud_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.RevertCloudSearch);

    private void ApplyBing_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.ApplyBingSearch);

    private void RevertBing_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.RevertBingSearch);

    private void ApplyWebResults_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.ApplyWebResults);

    private void RevertWebResults_Click(object sender, RoutedEventArgs e) =>
        ExecuteRegistryOperation(_registryService.RevertWebResults);

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
            ShowNotification(T("RestartExplorerSuccessTitle"), T("RestartExplorerSuccessMessage"), SymbolRegular.CheckmarkCircle24, GetThemeBrush("StatusSuccessBrush"));
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
        try
        {
            _themeService.ChangeTheme(preference, this);
            RefreshThemeSelection();
            RefreshStatus();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void ExecuteRegistryOperation(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            RefreshStatus();
        }
    }

    private void ShowError(Exception ex)
    {
        ShowNotification(
            T("ErrorTitle"),
            string.Format(CultureInfo.CurrentCulture, T("ErrorMessage"), ex.Message),
            SymbolRegular.DismissCircle24,
            GetThemeBrush("StatusDangerBrush"));
    }

    private void ShowNotification(string title, string message, SymbolRegular symbol, Brush iconBrush)
    {
        NotificationTitleText.Text = title;
        NotificationMessageText.Text = message;
        NotificationIcon.Symbol = symbol;
        NotificationIcon.Foreground = iconBrush;
        NotificationCloseButton.Content = T("Close");
        NotificationOverlay.Visibility = Visibility.Visible;
    }

    private void NotificationOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        NotificationOverlay.Visibility = Visibility.Collapsed;
    }

    private void NotificationPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void NotificationCloseButton_Click(object sender, RoutedEventArgs e)
    {
        NotificationOverlay.Visibility = Visibility.Collapsed;
    }

    private string T(string key) => _localizationService.GetString(key);

    protected override void OnClosed(EventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        _themeService.ThemeApplied -= ThemeService_ThemeApplied;
        _themeService.Dispose();

        base.OnClosed(e);
    }
}

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

    // Bolt: Cache frozen brushes to avoid unnecessary allocations on UI updates
    private static readonly SolidColorBrush SuccessBrush = CreateFrozenBrush(Color.FromRgb(0x0f, 0x7b, 0x0f));
    private static readonly SolidColorBrush DangerBrush = CreateFrozenBrush(Color.FromRgb(0xc4, 0x2b, 0x1c));

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public MainWindow()
    {
        InitializeComponent();

        ApplicationThemeManager.Apply(ApplicationTheme.Light);
        SystemThemeWatcher.Watch(this, WindowBackdropType.Mica, updateAccents: true);

        Loaded += MainWindow_Loaded;
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
        LanguageButton.ToolTip = T("LanguageTooltip");

        LanguageDialogTitleText.Text = T("LanguageDialogTitle");
        LanguageDialogDescriptionText.Text = T("LanguageDialogDescription");
        PortugueseButton.Content = T("PortugueseBrazil");
        EnglishButton.Content = T("English");
        CloseLanguageButton.Content = T("Close");

        RefreshLanguageSelection();
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
                (T("Optimized"), SymbolRegular.CheckmarkCircle24, SuccessBrush),

            RegistryService.OptimizationStatus.NotOptimized =>
                (T("NotOptimized"), SymbolRegular.DismissCircle24, DangerBrush),

            _ =>
                (T("Unknown"), SymbolRegular.QuestionCircle24, Brushes.Gray)
        };
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
        var result = System.Windows.MessageBox.Show(
            T("RestartExplorerConfirmMessage"),
            T("RestartExplorerConfirmTitle"),
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
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
    }

    private void LanguageButton_Click(object sender, RoutedEventArgs e)
    {
        LanguageOverlay.Visibility = Visibility.Visible;
    }

    private void PortugueseButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeLanguage("pt-BR");
    }

    private void EnglishButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeLanguage("en-US");
    }

    private void CloseLanguageButton_Click(object sender, RoutedEventArgs e)
    {
        LanguageOverlay.Visibility = Visibility.Collapsed;
    }

    private void LanguageOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LanguageOverlay.Visibility = Visibility.Collapsed;
    }

    private static void LanguagePanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ChangeLanguage(string cultureName)
    {
        _localizationService.ApplyCulture(cultureName);
        ApplyLocalization();
    }

    private string T(string key) => _localizationService.GetString(key);

    private void ShowError(Exception ex)
    {
        System.Windows.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, T("ErrorMessage"), ex.Message), T("ErrorTitle"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }
}

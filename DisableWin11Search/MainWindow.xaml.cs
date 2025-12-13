using System.Windows;
using System.Windows.Media;
using DisableWin11Search.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DisableWin11Search
{
    public partial class MainWindow : FluentWindow
    {
        private readonly RegistryService _registryService;

        public MainWindow()
        {
            InitializeComponent();
            _registryService = new RegistryService();
            
            // Force Light Theme
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            UpdateStatusUI(IconSuggestions, TextSuggestions, _registryService.CheckSearchBoxSuggestions());
            UpdateStatusUI(IconCloud, TextCloud, _registryService.CheckCloudSearch());
            UpdateStatusUI(IconBing, TextBing, _registryService.CheckBingSearch());
        }

        private void UpdateStatusUI(SymbolIcon icon, System.Windows.Controls.TextBlock textBlock, RegistryService.OptimizationStatus status)
        {
            // Colors using System.Windows.Media.Brushes
            // Success: #00cc6a (Green)
            // Danger: #e81123 (Red)
            // Caution: #fce100 (Yellow/Gold) or #ffb900

            switch (status)
            {
                case RegistryService.OptimizationStatus.Optimized:
                    textBlock.Text = "Otimizado";
                    icon.Symbol = SymbolRegular.CheckmarkCircle24;
                    icon.Foreground = new SolidColorBrush(Color.FromRgb(0x0f, 0x7b, 0x0f)); // Darker Green for Light Theme
                    break;
                case RegistryService.OptimizationStatus.NotOptimized:
                    textBlock.Text = "Não Otimizado";
                    icon.Symbol = SymbolRegular.DismissCircle24;
                    icon.Foreground = new SolidColorBrush(Color.FromRgb(0xc4, 0x2b, 0x1c)); // Red
                    break;
                default:
                    textBlock.Text = "Desconhecido";
                    icon.Symbol = SymbolRegular.QuestionCircle24;
                    icon.Foreground = Brushes.Gray;
                    break;
            }
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

        private void RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "Isso irá encerrar o Windows Explorer. A barra de tarefas desaparecerá momentaneamente.\nDeseja continuar?",
                "Confirmar Reinício",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    _registryService.RestartExplorer();
                    System.Windows.MessageBox.Show("Explorer reiniciado.", "Sucesso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }

        private void ShowError(Exception ex)
        {
            System.Windows.MessageBox.Show($"Erro: {ex.Message}", "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

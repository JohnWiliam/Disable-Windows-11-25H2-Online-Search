using System.Globalization;
using System.Windows;
using DisableWin11Search.Services;

namespace DisableWin11Search;

public partial class App : Application
{
    private readonly LocalizationService _localizationService = new();

    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show(
            string.Format(CultureInfo.CurrentCulture, T("UiUnhandledExceptionMessage"), e.Exception.Message, e.Exception.StackTrace),
            T("UiUnhandledExceptionTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        System.Windows.MessageBox.Show(
            string.Format(CultureInfo.CurrentCulture, T("DomainUnhandledExceptionMessage"), ex?.Message ?? T("DomainUnhandledUnknown"), ex?.StackTrace),
            T("DomainUnhandledExceptionTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private string T(string key) => _localizationService.GetString(key);
}

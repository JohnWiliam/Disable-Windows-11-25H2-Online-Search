using System.Windows;
using DisableWin11Search.Services;
// using Wpf.Ui.Controls; // Removed to avoid ambiguity

namespace DisableWin11Search
{
    public partial class App : Application
    {
        public App()
        {
            // Hook up global exception handling to catch startup crashes
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(
                $"An unhandled UI exception occurred: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                "Application Startup Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            
            // Prevent default crash if possible
            e.Handled = true; 
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            System.Windows.MessageBox.Show(
                $"An unhandled domain exception occurred: {ex?.Message ?? "Unknown Error"}\n\nStack Trace:\n{ex?.StackTrace}", 
                "Critical Application Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
}

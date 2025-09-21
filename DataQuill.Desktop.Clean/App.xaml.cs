using System.Configuration;
using System.Data;
using System.Windows;

namespace DataQuillDesktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Console.WriteLine("App.OnStartup: Starting application...");
            base.OnStartup(e);
            Console.WriteLine("App.OnStartup: Base startup completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"App.OnStartup: Exception caught - {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Application startup error: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown(1);
        }
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Unhandled exception: {e.Exception.Message}\n\nStack trace: {e.Exception.StackTrace}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
        this.Shutdown(1);
    }
}


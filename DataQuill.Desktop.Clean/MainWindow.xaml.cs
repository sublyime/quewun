using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using DataQuillDesktop.ViewModels;
using DataQuillDesktop.Services;

namespace DataQuillDesktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            Console.WriteLine("=== DataQuill Desktop Starting ===");
            InitializeComponent();
            Console.WriteLine("Window initialized successfully!");

            // Keep it simple for now - just show the window
            this.Show();
            this.Activate();
            this.WindowState = WindowState.Normal;
            this.Topmost = true;
            this.Focus();

            Console.WriteLine("Window should now be visible!");
            Console.WriteLine("=== DataQuill Desktop Ready ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing MainWindow: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Error starting application: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                "DataQuill Desktop Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void NavigateToSection(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string section = button.Name.Replace("Btn", "");

            // Hide all panels first
            ContentDisplay.Visibility = Visibility.Collapsed;
            DashboardPanel.Visibility = Visibility.Collapsed;
            DataSourcesPanel.Visibility = Visibility.Collapsed;
            ReportsPanel.Visibility = Visibility.Collapsed;
            StoragePanel.Visibility = Visibility.Collapsed;
            UsersPanel.Visibility = Visibility.Collapsed;
            TerminalPanel.Visibility = Visibility.Collapsed;

            // Show the appropriate content
            switch (section)
            {
                case "Dashboard":
                    DashboardPanel.Visibility = Visibility.Visible;
                    break;
                case "DataSources":
                    DataSourcesPanel.Visibility = Visibility.Visible;
                    break;
                case "Reports":
                    ReportsPanel.Visibility = Visibility.Visible;
                    break;
                case "Storage":
                    StoragePanel.Visibility = Visibility.Visible;
                    break;
                case "Users":
                    UsersPanel.Visibility = Visibility.Visible;
                    break;
                case "Terminal":
                    TerminalPanel.Visibility = Visibility.Visible;
                    break;
                default:
                    ContentDisplay.Visibility = Visibility.Visible;
                    break;
            }

            // Update title to show current section
            this.Title = $"DataQuill Desktop - {section}";
        }
    }
}
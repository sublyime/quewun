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

            // Show the appropriate content
            if (section == "Dashboard")
            {
                DashboardPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ContentDisplay.Visibility = Visibility.Visible;
                string content = section switch
                {
                    "Configure" => "⚙️ Configuration Section\n\nManage:\n• Database connections\n• Application settings\n• User preferences\n• System configuration",
                    "Reports" => "📄 Reports Section\n\nAccess:\n• Generate custom reports\n• Export to PDF/Excel\n• Schedule automated reports\n• Report templates",
                    "Storage" => "💾 Storage Section\n\nManage:\n• File operations\n• Backup and restore\n• Data import/export\n• Storage monitoring",
                    "Users" => "👥 Users Section\n\nAdminister:\n• User accounts\n• Permissions and roles\n• Authentication settings\n• User profiles",
                    "Terminal" => "💻 Terminal Section\n\nExecute:\n• SQL queries\n• Command operations\n• Query history\n• Results visualization",
                    _ => "Welcome to DataQuill Desktop! Click a section on the left to get started."
                };
                ContentDisplay.Text = content;
            }

            // Update title to show current section
            this.Title = $"DataQuill Desktop - {section}";
        }
    }
}
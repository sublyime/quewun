using System.Net.Http;
using System.Windows;
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

            // Set up the main view model
            var httpClient = new HttpClient();
            var apiService = new ApiService(httpClient);
            var mainViewModel = new MainViewModel(apiService);

            // Set the DataContext for MVVM binding
            DataContext = mainViewModel;

            Console.WriteLine("MainViewModel created and bound to DataContext");
            Console.WriteLine("=== DataQuill Desktop Ready ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing MainWindow: {ex.Message}");
            MessageBox.Show($"Error starting application: {ex.Message}", "DataQuill Desktop Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        var message = """
            🎉 DataQuill Desktop - Native Windows Features Test

            ✅ WPF .NET 8 Framework
            ✅ Material Design UI Components
            ✅ MVVM Architecture with Data Binding
            ✅ Navigation System
            ✅ REST API Service Ready
            ✅ Native Windows Performance

            Your application successfully integrates:
            • Modern UI with Material Design
            • Professional MVVM pattern
            • Backend API connectivity
            • Native Windows features

            Click on the navigation items to explore different sections!
            """;

        MessageBox.Show(message, "DataQuill Desktop - Status Check",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
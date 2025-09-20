using System;
using System.Net.Http;
using System.Windows;

namespace DataQuill.Desktop;

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
        MessageBox.Show("Success! Your native Windows application is working perfectly!",
                       "DataQuill Desktop",
                       MessageBoxButton.OK,
                       MessageBoxImage.Information);
    }
}

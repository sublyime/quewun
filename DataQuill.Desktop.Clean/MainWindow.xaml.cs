using System;
using System.Windows;

namespace DataQuillDesktop
{
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
                Console.WriteLine("Error initializing MainWindow: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                MessageBox.Show("Error starting application: " + ex.Message, "DataQuill Desktop Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void ShowStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Showing Storage content...");
                WelcomeText.Visibility = Visibility.Collapsed;
                StorageContent.Visibility = Visibility.Visible;
                Console.WriteLine("Storage content should now be visible");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error showing storage: " + ex.Message);
                MessageBox.Show("Error showing storage: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TestModbus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Starting Modbus TCP test...");
                await TestModbus.TestModbusTcp();
                MessageBox.Show("Modbus TCP test completed! Check the console output for results.", "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running Modbus test: " + ex.Message);
                MessageBox.Show("Error running Modbus test: " + ex.Message, "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Windows;

namespace DataQuillDesktop
{
    public partial class SimpleTestWindow : Window
    {
        public SimpleTestWindow()
        {
            Console.WriteLine("=== Simple Test Window Starting ===");

            try
            {
                // Create a minimal window programmatically
                this.Title = "DataQuill Test";
                this.Width = 600;
                this.Height = 400;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                var grid = new System.Windows.Controls.Grid();
                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = "✅ Application is running!\n\nThis simple test confirms the WPF framework is working.",
                    FontSize = 16,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20)
                };

                grid.Children.Add(textBlock);
                this.Content = grid;

                Console.WriteLine("✅ Simple test window created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in simple test window: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
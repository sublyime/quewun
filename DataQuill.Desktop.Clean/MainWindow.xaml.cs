using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;
using DataQuillDesktop.Commands;

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
                Console.WriteLine("XAML loaded successfully");

                // No need for complex ViewModels - we'll handle navigation directly
                Console.WriteLine("Using simplified navigation system");

                // Initialize to Dashboard
                NavigateToSection("Dashboard");

                Console.WriteLine("Window initialized successfully!");
                Console.WriteLine("=== DataQuill Desktop Ready ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during initialization: {ex.Message}");
                MessageBox.Show($"Critical startup error: {ex.Message}", "DataQuill Desktop", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("Using emergency fallback");
            }
        }

        // Simple Click Event Handlers
        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("Dashboard");
        }

        private void DataSourcesBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("DataSources");
        }

        private void StorageBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("Storage");
        }

        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("Reports");
        }

        private void TerminalBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("Terminal");
        }

        private void UserAdminBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("UserAdmin");
        }

        private void UsersBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSection("Users");
        }

        // Simple Navigation Method
        private void NavigateToSection(string section)
        {
            Console.WriteLine($"Navigating to section: {section}");

            // Update the current section display
            CurrentSectionDisplay.Text = $"Current Section: {section}";

            // Create content based on section
            Border content = CreateContentForSection(section);

            // Set the content
            MainContentControl.Content = content;

            Console.WriteLine($"Navigation to {section} completed successfully");
        }

        // Create Content for Each Section
        private Border CreateContentForSection(string section)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel();

            switch (section)
            {
                case "Dashboard":
                    border.Background = Brushes.LightGreen;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "📊 Dashboard",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Real-time monitoring and analytics",
                        FontSize = 18,
                        Margin = new Thickness(0, 0, 0, 15)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Status: All systems operational",
                        FontSize = 14,
                        Foreground = Brushes.DarkGreen
                    });
                    break;

                case "DataSources":
                    border.Background = Brushes.LightBlue;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "🔌 Data Sources",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Configure and manage data connections",
                        FontSize = 18
                    });
                    break;

                case "Storage":
                    border.Background = Brushes.LightYellow;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "💾 Storage",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Data storage and backup management",
                        FontSize = 18
                    });
                    break;

                case "Reports":
                    border.Background = Brushes.LightCoral;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "📊 Reports",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Generate and export data reports",
                        FontSize = 18
                    });
                    break;

                case "Terminal":
                    border.Background = Brushes.Black;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "💻 Terminal",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.LimeGreen,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Command line interface and system access",
                        FontSize = 18,
                        Foreground = Brushes.White
                    });
                    break;

                case "UserAdmin":
                    border.Background = Brushes.LightSteelBlue;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "👥 User Administration",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Manage user accounts and permissions",
                        FontSize = 18
                    });
                    break;

                case "Users":
                    border.Background = Brushes.Lavender;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "👤 Users",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "User profiles and account settings",
                        FontSize = 18
                    });
                    break;

                default:
                    border.Background = Brushes.LightGray;
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "Unknown Section",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold
                    });
                    break;
            }

            border.Child = stackPanel;
            return border;
        }
    }

    // Simple fallback ViewModel that provides basic navigation
    public class FallbackViewModel : INotifyPropertyChanged
    {
        private string _currentSection = "Dashboard";

        public string CurrentSection
        {
            get => _currentSection;
            set
            {
                _currentSection = value;
                OnPropertyChanged();
                Console.WriteLine($"CurrentSection changed to: {value}");
            }
        }

        public string ConnectionStatus { get; set; } = "Offline";
        public bool IsConnected { get; set; } = false;
        public string WelcomeMessage { get; set; } = "DataQuill Desktop - Safe Mode";
        public ICommand NavigateCommand { get; }

        public FallbackViewModel()
        {
            NavigateCommand = new RelayCommand<string>(Navigate);
        }

        private void Navigate(string? section)
        {
            Console.WriteLine($"Navigate called with parameter: '{section}'");
            if (!string.IsNullOrEmpty(section))
            {
                var oldSection = CurrentSection;
                CurrentSection = section;
                Console.WriteLine($"Section changed from '{oldSection}' to '{CurrentSection}'");
                Console.WriteLine($"PropertyChanged event will be fired for CurrentSection");
            }
            else
            {
                Console.WriteLine("Section parameter was null or empty - no navigation performed");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            Console.WriteLine($"OnPropertyChanged called for property: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine($"PropertyChanged event fired for: {propertyName}");
        }
    }
}

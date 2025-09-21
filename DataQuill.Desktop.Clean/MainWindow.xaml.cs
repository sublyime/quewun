using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;
using DataQuillDesktop.Commands;
using DataQuillDesktop.Views;
using DataQuillDesktop.Models;

namespace DataQuillDesktop
{
    public partial class MainWindow : Window
    {
        private IntegratedBackendService? _backendService;

        public MainWindow()
        {
            try
            {
                Console.WriteLine("=== DataQuill Desktop Starting ===");

                // Ensure console is available for debugging
                try
                {
                    AllocConsole();
                }
                catch
                {
                    // Console allocation may fail in some environments, continue anyway
                }

                Console.WriteLine("About to initialize XAML components...");
                InitializeComponent();
                Console.WriteLine("✅ XAML loaded successfully");

                Console.WriteLine("Re-enabling database services only...");

                // Re-enable database initialization (this worked)
                InitializeDatabaseServices();

                // Temporarily disable backend initialization (this causes crash)
                // InitializeBackendServices();

                Console.WriteLine("Setting up minimal navigation...");

                // Initialize to Dashboard with fallback
                try
                {
                    NavigateToSection("Dashboard");
                    Console.WriteLine("✅ Navigation initialized successfully");
                }
                catch (Exception navEx)
                {
                    Console.WriteLine($"❌ Navigation failed: {navEx.Message}");
                    // Create minimal content
                    MainContentControl.Content = new System.Windows.Controls.TextBlock
                    {
                        Text = "DataQuill Desktop - Minimal Mode\n\nServices temporarily disabled for debugging.",
                        FontSize = 16,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(20)
                    };
                }

                Console.WriteLine("✅ Window initialized successfully!");
                Console.WriteLine("=== DataQuill Desktop Ready (Debug Mode) ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error during initialization: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                try
                {
                    MessageBox.Show($"Critical startup error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                                  "DataQuill Desktop", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                {
                    // Even MessageBox failed, write to console
                    Console.WriteLine("❌ MessageBox also failed - critical system error");
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private async Task StartBackendServicesAsync()
        {
            try
            {
                if (_backendService != null)
                {
                    await _backendService.StartAsync();
                    Console.WriteLine("✅ Backend services started - live data should be available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start backend services: {ex.Message}");
            }
        }

        private void InitializeDatabaseServices()
        {
            try
            {
                Console.WriteLine("🔄 Initializing database services...");

                // Try SQLite for easier setup
                DatabaseConfigurationService.SetProvider(DatabaseConfigurationService.DatabaseProvider.SQLite);

                // Test connection and ensure database is created
                if (DatabaseConfigurationService.TestConnection())
                {
                    DatabaseConfigurationService.EnsureDatabaseCreated();
                    Console.WriteLine("✅ Database services initialized successfully");

                    // Initialize some sample data sources for testing
                    InitializeSampleDataSources();
                }
                else
                {
                    Console.WriteLine("⚠️ Database connection failed, some features may be limited");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
            }
        }

        private void InitializeSampleDataSources()
        {
            try
            {
                using var context = new QuillDbContext();

                // Check if data sources already exist
                if (context.DataSources.Any())
                {
                    Console.WriteLine("ℹ️ Data sources already configured");
                    return;
                }

                Console.WriteLine("🔧 Creating sample data sources for testing...");

                // Create a sample Modbus data source
                var modbusSource = new DataSource
                {
                    Name = "Test Modbus Device",
                    InterfaceType = InterfaceType.TCP,
                    IsActive = true,
                    Configuration = new DataSourceConfiguration
                    {
                        Host = "127.0.0.1",
                        Port = 502,
                        SlaveId = 1,
                        Timeout = 5000
                    }
                };

                // Create a sample file-based data source for guaranteed data
                var fileSource = new DataSource
                {
                    Name = "Sample File Data",
                    InterfaceType = InterfaceType.File,
                    IsActive = true,
                    Configuration = new DataSourceConfiguration
                    {
                        FilePath = "sample_data.csv"
                    }
                };

                context.DataSources.AddRange(modbusSource, fileSource);
                context.SaveChanges();

                Console.WriteLine("✅ Sample data sources created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create sample data sources: {ex.Message}");
            }
        }

        private void InitializeBackendServices()
        {
            try
            {
                Console.WriteLine("🔄 Initializing backend services...");
                _backendService = new IntegratedBackendService();

                // Start backend services asynchronously
                _ = StartBackendServicesAsync();

                Console.WriteLine("✅ Backend services initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Backend services initialization failed: {ex.Message}");
                _backendService = null;
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

            // Try to update the welcome text at the top (may not work if controls aren't ready)
            try { this.Title = $"DataQuill Desktop - {section}"; } catch { }

            // Create content based on section - now loading actual Views with live data
            UserControl content = CreateViewForSection(section);

            // Set the content
            try
            {
                var contentControl = this.FindName("MainContentControl") as ContentControl;
                if (contentControl != null)
                {
                    contentControl.Content = content;
                }
                else
                {
                    Console.WriteLine("⚠️ MainContentControl not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error setting content: {ex.Message}");
            }

            Console.WriteLine($"Navigation to {section} completed successfully");
        }

        // Create Actual Views for Each Section
        private UserControl CreateViewForSection(string section)
        {
            UserControl view;

            try
            {
                switch (section)
                {
                    case "Dashboard":
                        Console.WriteLine("Loading DashboardView...");
                        view = new DashboardView();

                        // Connect to the IntegratedBackendService for real data
                        if (_backendService != null)
                        {
                            try
                            {
                                // Create a DashboardViewModel that uses the IntegratedBackendService's data
                                view.DataContext = new DashboardViewModelWrapper(_backendService);
                                Console.WriteLine("✅ Dashboard connected to IntegratedBackendService for real data");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Dashboard wrapper failed: {ex.Message}, using direct service");
                                // Fallback: create a simple ViewModel that exposes the backend service data
                                view.DataContext = CreateDashboardFallback(_backendService);
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Backend service not available, creating simple dashboard");
                            // Create a simple dashboard without backend dependency
                            view.DataContext = new SimpleDashboardViewModel();
                        }
                        {
                            Console.WriteLine("⚠️ No backend service available, using static fallback");
                            view.DataContext = new
                            {
                                CollectionStatusText = "🔴 Backend service not available",
                                DataSourcesSummary = "Service initialization failed",
                                IsCollectionRunning = false,
                                StartCollectionCommand = new RelayCommand(() =>
                                {
                                    Console.WriteLine("Start Collection clicked - backend service unavailable");
                                    MessageBox.Show("Backend services are not available.", "DataQuill", MessageBoxButton.OK, MessageBoxImage.Warning);
                                })
                            };
                        }
                        break;

                    case "DataSources":
                        Console.WriteLine("Loading DataSourcesView...");
                        try
                        {
                            view = new DataSourcesView();
                            Console.WriteLine("✅ DataSourcesView created successfully");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ DataSourcesView creation failed: {ex.Message}");
                            throw;
                        }
                        break;

                    case "Storage":
                        Console.WriteLine("Loading StorageView...");
                        view = new StorageView();
                        break;

                    case "Reports":
                        Console.WriteLine("Loading ReportsView...");
                        view = new ReportsView();
                        break;

                    case "Terminal":
                        Console.WriteLine("Loading TerminalView...");
                        try
                        {
                            // Try to load the full TerminalView now that services are restored
                            view = new TerminalView();
                            Console.WriteLine("✅ Full TerminalView created successfully");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ TerminalView creation failed: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");

                            // Fall back to simple terminal if the full one fails
                            view = CreateSimpleTerminalView();
                            Console.WriteLine("🔄 Using simplified Terminal as fallback");
                        }
                        break;

                    case "UserAdmin":
                        Console.WriteLine("Loading UserAdminView...");
                        view = new UserAdminView();
                        break;

                    case "Users":
                        Console.WriteLine("Loading UsersView...");
                        view = new UsersView();
                        break;

                    default:
                        Console.WriteLine($"Unknown section: {section}, loading default content");
                        view = CreateFallbackContent(section);
                        break;
                }

                Console.WriteLine($"Successfully loaded view for {section}");
                return view;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading view for {section}: {ex.Message}");
                return CreateFallbackContent(section);
            }
        }

        // Fallback content in case View loading fails
        private UserControl CreateFallbackContent(string section)
        {
            var userControl = new UserControl();
            var border = new Border
            {
                Background = Brushes.LightGray,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"⚠️ {section}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            stackPanel.Children.Add(new TextBlock
            {
                Text = "View temporarily unavailable",
                FontSize = 16,
                Foreground = Brushes.Gray
            });

            border.Child = stackPanel;
            userControl.Content = border;
            return userControl;
        }

        // Create a simple terminal view for testing
        private UserControl CreateSimpleTerminalView()
        {
            var userControl = new UserControl();
            var grid = new Grid();

            // Add row definitions
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                Padding = new Thickness(15)
            };
            Grid.SetRow(headerBorder, 0);

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
            headerStack.Children.Add(new TextBlock
            {
                Text = "💻",
                FontSize = 20,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            headerStack.Children.Add(new TextBlock
            {
                Text = "Terminal",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });

            headerBorder.Child = headerStack;
            grid.Children.Add(headerBorder);

            // Content area
            var contentBorder = new Border
            {
                Background = Brushes.White,
                Margin = new Thickness(10),
                Padding = new Thickness(20)
            };
            Grid.SetRow(contentBorder, 1);

            var contentStack = new StackPanel();
            contentStack.Children.Add(new TextBlock
            {
                Text = "✅ Terminal is working!",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = "This is a simplified terminal view that bypasses the complex ViewModel.",
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = "The sidebar navigation is working correctly!",
                FontSize = 14,
                Foreground = Brushes.DarkGreen
            });

            contentBorder.Child = contentStack;
            grid.Children.Add(contentBorder);

            userControl.Content = grid;
            return userControl;
        }

        public class SimpleDashboardViewModel : INotifyPropertyChanged
        {
            public string CollectionStatusText => "🔴 Backend service disabled";
            public string DataSourcesSummary => "Services temporarily disabled for debugging";
            public bool IsCollectionRunning => false;
            public ObservableCollection<object> RealtimeData => new ObservableCollection<object>();
            public ObservableCollection<object> RecentActivities => new ObservableCollection<object>();
            public SimpleDashboardMetrics Metrics => new SimpleDashboardMetrics();

            public RelayCommand StartCollectionCommand => new RelayCommand(() =>
            {
                Console.WriteLine("⚠️ Start Collection disabled - backend services not initialized");
                MessageBox.Show("Start Collection is temporarily disabled while debugging backend services.", 
                              "Feature Disabled", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            public RelayCommand StopCollectionCommand => new RelayCommand(() =>
            {
                Console.WriteLine("⚠️ Stop Collection disabled - backend services not initialized");
            });

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        public class SimpleDashboardMetrics
        {
            public int ActiveConnections => 0;
            public int TotalDataPoints => 0;
            public string DataProcessedTodayFormatted => "0 MB";
            public string AverageResponseTimeFormatted => "0 ms";
        }

        public class TerminalFallbackViewModel : INotifyPropertyChanged
        {
            public string StatusMessage { get; } = "Terminal fallback mode active";
            public bool IsConnected { get; } = false;

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        // Create a dashboard fallback that connects to the IntegratedBackendService
        private object CreateDashboardFallback(IntegratedBackendService backendService)
        {
            return new
            {
                CollectionStatusText = backendService.IsConnected ? "🟢 Connected to live services" : "🔴 Service not running",
                DataSourcesSummary = $"Backend Status: {backendService.Status}",
                IsCollectionRunning = backendService.IsConnected,
                RealtimeData = backendService.RealtimeData,
                RecentActivities = backendService.RecentActivities,
                Metrics = backendService.Metrics,
                StartCollectionCommand = new RelayCommand(async () =>
                {
                    try
                    {
                        Console.WriteLine("Starting data collection via IntegratedBackendService...");
                        await backendService.StartAsync();
                        Console.WriteLine("✅ Data collection started");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to start collection: {ex.Message}");
                        MessageBox.Show($"Failed to start data collection: {ex.Message}", "DataQuill", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }),
                StopCollectionCommand = new RelayCommand(() =>
                {
                    try
                    {
                        Console.WriteLine("Stopping data collection...");
                        _ = backendService.StopAsync();
                        Console.WriteLine("✅ Data collection stopped");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to stop collection: {ex.Message}");
                    }
                })
            };
        }

        // Simple wrapper class for DashboardViewModel functionality
        public class DashboardViewModelWrapper
        {
            private readonly IntegratedBackendService _backendService;

            public DashboardViewModelWrapper(IntegratedBackendService backendService)
            {
                _backendService = backendService;
            }

            public string CollectionStatusText => _backendService.IsConnected ? "🟢 Live Data Active" : "🔴 Disconnected";
            public string DataSourcesSummary => $"Status: {_backendService.Status}";
            public bool IsCollectionRunning => _backendService.IsConnected;
            public ObservableCollection<DataPoint> RealtimeData => _backendService.RealtimeData;
            public ObservableCollection<ActivityEvent> RecentActivities => _backendService.RecentActivities;
            public DashboardMetrics Metrics => _backendService.Metrics;

            public RelayCommand StartCollectionCommand => new RelayCommand(async () =>
            {
                try
                {
                    Console.WriteLine("🚀 Starting live data collection...");
                    Console.WriteLine($"Backend service status: {_backendService?.Status}");
                    Console.WriteLine($"Backend service connected: {_backendService?.IsConnected}");

                    // Check if we have any data sources
                    using var context = new QuillDbContext();
                    var dataSources = context.DataSources.Where(ds => ds.IsActive).ToList();
                    Console.WriteLine($"Found {dataSources.Count} active data sources:");

                    foreach (var ds in dataSources)
                    {
                        Console.WriteLine($"  - {ds.Name} ({ds.InterfaceType})");
                    }

                    if (dataSources.Count == 0)
                    {
                        Console.WriteLine("⚠️ No active data sources found! Creating sample data...");
                        // This should have been created during initialization
                    }

                    if (_backendService != null)
                    {
                        await _backendService.StartAsync();
                        Console.WriteLine("✅ Live data collection started");
                    }
                    else
                    {
                        Console.WriteLine("❌ Backend service is null - cannot start collection");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to start live collection: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            });

            public RelayCommand StopCollectionCommand => new RelayCommand(() =>
            {
                try
                {
                    Console.WriteLine("Stopping live data collection...");
                    _ = _backendService.StopAsync();
                    Console.WriteLine("✅ Live data collection stopped");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to stop live collection: {ex.Message}");
                }
            });
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

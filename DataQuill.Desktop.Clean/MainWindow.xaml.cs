using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
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

        // Public property to allow debugging access
        public IntegratedBackendService? BackendService => _backendService;

        public MainWindow()
        {
            try
            {
                Console.WriteLine("🚀 DataQuill Desktop starting...");
                InitializeComponent();
                Console.WriteLine("✅ InitializeComponent completed");

                // Initialize database first
                try
                {
                    InitializeSampleDataSources();
                    Console.WriteLine("✅ Database initialization completed");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"⚠️ Database initialization failed: {dbEx.Message}");
                    Console.WriteLine($"Stack trace: {dbEx.StackTrace}");
                }

                // Initialize backend services
                try
                {
                    InitializeBackendServices();
                    Console.WriteLine("✅ Backend services initialization completed");
                }
                catch (Exception backendEx)
                {
                    Console.WriteLine($"⚠️ Backend services initialization failed: {backendEx.Message}");
                    Console.WriteLine($"Stack trace: {backendEx.StackTrace}");
                }

                // Load the initial Dashboard view
                try
                {
                    NavigateToSection("Dashboard");
                    Console.WriteLine("✅ Initial Dashboard navigation completed");
                }
                catch (Exception navEx)
                {
                    Console.WriteLine($"⚠️ Initial navigation failed: {navEx.Message}");
                    Console.WriteLine($"Stack trace: {navEx.StackTrace}");

                    // Fallback: show a simple message
                    try
                    {
                        MainContentControl.Content = new System.Windows.Controls.TextBlock
                        {
                            Text = "DataQuill Desktop - Safe Mode\nNavigation failed, but application is running.",
                            FontSize = 16,
                            Margin = new Thickness(20),
                            TextWrapping = TextWrapping.Wrap
                        };
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"⚠️ Even fallback content failed: {fallbackEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error in MainWindow constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Try to show error to user
                try
                {
                    MessageBox.Show($"Application startup failed: {ex.Message}", "DataQuill Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                {
                    Console.WriteLine("❌ Could not even show error message to user");
                }

                // Don't let the app crash silently
                Environment.Exit(1);
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

                    // Update existing Modbus source to have correct protocol type
                    var existingModbusSource = context.DataSources.FirstOrDefault(ds => ds.Name == "Test Modbus Device");
                    if (existingModbusSource != null && existingModbusSource.ProtocolType != ProtocolType.ModbusTCP)
                    {
                        Console.WriteLine("🔧 Updating Modbus source protocol type...");
                        existingModbusSource.ProtocolType = ProtocolType.ModbusTCP;
                        context.SaveChanges();
                        Console.WriteLine("✅ Modbus source protocol type updated");
                    }

                    // MANUAL TRIGGER: Force enhanced configuration setup
                    Console.WriteLine("🚀 MANUAL TRIGGER: Checking and forcing enhanced Modbus configuration...");
                    DatabaseConfigurationService.CheckModbusConfigurationStatus();
                    DatabaseConfigurationService.ForceEnhancedModbusSetup();

                    return;
                }

                Console.WriteLine("🔧 Creating sample data sources for testing...");

                // Create a sample Modbus data source
                var modbusSource = new DataSource
                {
                    Name = "Test Modbus Device",
                    InterfaceType = InterfaceType.TCP,
                    ProtocolType = ProtocolType.ModbusTCP,
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
                Console.WriteLine("🔄 Initializing backend services with detailed error tracking...");

                // Also write to a log file for debugging
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Starting backend initialization\n");

                try
                {
                    Console.WriteLine("🔄 Creating IntegratedBackendService...");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Creating IntegratedBackendService...\n");

                    _backendService = new IntegratedBackendService();

                    Console.WriteLine("✅ IntegratedBackendService created successfully");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] IntegratedBackendService created successfully\n");
                }
                catch (Exception serviceEx)
                {
                    var errorMsg = $"❌ Failed to create IntegratedBackendService: {serviceEx.Message}";
                    var stackMsg = $"Stack trace: {serviceEx.StackTrace}";
                    var innerMsg = $"Inner exception: {serviceEx.InnerException?.Message}";

                    Console.WriteLine(errorMsg);
                    Console.WriteLine(innerMsg);
                    Console.WriteLine(stackMsg);

                    File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMsg}\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] {innerMsg}\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] {stackMsg}\n");

                    _backendService = null;
                    return;
                }

                // Try to start services only if creation succeeded
                if (_backendService != null)
                {
                    try
                    {
                        Console.WriteLine("🔄 Starting backend services asynchronously...");
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Starting backend services...\n");

                        _ = StartBackendServicesAsync();

                        Console.WriteLine("✅ Backend services start initiated");
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Backend services start initiated\n");
                    }
                    catch (Exception startEx)
                    {
                        var errorMsg = $"❌ Failed to start backend services: {startEx.Message}";
                        Console.WriteLine(errorMsg);
                        Console.WriteLine($"Stack trace: {startEx.StackTrace}");
                        File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMsg}\n");
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Stack trace: {startEx.StackTrace}\n");
                        // Keep the service but note it couldn't start
                    }
                }

                Console.WriteLine("✅ Backend services initialization completed");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Backend services initialization completed\n");
            }
            catch (Exception ex)
            {
                var errorMsg = $"❌ Backend services initialization failed: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                try
                {
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMsg}\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Stack trace: {ex.StackTrace}\n");
                }
                catch { /* Ignore logging errors */ }

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

        private void EnhancedModbusBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("🚀 MANUAL BUTTON: Enhanced Modbus configuration triggered by user");

                // Show status before
                DatabaseConfigurationService.CheckModbusConfigurationStatus();

                // Force enhanced setup
                DatabaseConfigurationService.ForceEnhancedModbusSetup();

                // Show status after
                DatabaseConfigurationService.CheckModbusConfigurationStatus();

                // Update UI to show success
                MessageBox.Show(
                    "Enhanced Modbus configuration has been applied!\n\n" +
                    "Check the console output for details.\n" +
                    "The next data collection cycle should use the new enhanced register configuration.",
                    "Enhanced Modbus Setup",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Console.WriteLine("✅ MANUAL BUTTON: Enhanced Modbus setup completed - check Dashboard for new tag names");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MANUAL BUTTON FAILED: {ex.Message}");
                MessageBox.Show(
                    $"Failed to apply enhanced Modbus configuration:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // Simple Navigation Method
        private void NavigateToSection(string section)
        {
            Console.WriteLine($"Navigating to section: {section}");

            // Try to update the welcome text at the top (may not work if controls aren't ready)
            try { this.Title = $"DataQuill Desktop - {section}"; } catch { }

            // Force clear the content area if navigating to Dashboard (to force refresh)
            if (section == "Dashboard")
            {
                try
                {
                    var contentControl = this.FindName("MainContentControl") as ContentControl;
                    if (contentControl != null)
                    {
                        contentControl.Content = null;
                        System.GC.Collect(); // Force cleanup of old ViewModels
                        Console.WriteLine("🔄 Dashboard content cleared for refresh");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error clearing Dashboard content: {ex.Message}");
                }
            }

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
                        Console.WriteLine($"Backend service status: {(_backendService != null ? "Available" : "NULL")}");

                        // Log to file as well
                        try
                        {
                            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Dashboard creation - Backend service: {(_backendService != null ? "Available" : "NULL")}\n");
                        }
                        catch { }

                        view = new DashboardView();

                        // Connect to the IntegratedBackendService for real data
                        if (_backendService != null)
                        {
                            try
                            {
                                Console.WriteLine("🔄 Attempting to create DashboardViewModelWrapper...");

                                // Log details about the backend service
                                Console.WriteLine($"Backend service type: {_backendService.GetType().Name}");
                                Console.WriteLine($"Backend service connected: {_backendService.IsConnected}");
                                Console.WriteLine($"Backend service status: {_backendService.Status}");

                                // Create a DashboardViewModel that uses the IntegratedBackendService's data
                                var dashboardWrapper = new DashboardViewModelWrapper(_backendService);
                                view.DataContext = dashboardWrapper;
                                Console.WriteLine("✅ Dashboard connected to IntegratedBackendService for real data");
                                Console.WriteLine($"✅ Dashboard DataContext type: {view.DataContext.GetType().Name}");
                                Console.WriteLine($"✅ Dashboard DataContext CollectionStatusText: {((DashboardViewModelWrapper)view.DataContext).CollectionStatusText}");

                                try
                                {
                                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] Dashboard connected to real backend service successfully - DataContext: {view.DataContext.GetType().Name}\n");
                                }
                                catch { }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Dashboard wrapper failed: {ex.Message}, using direct service");
                                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                                try
                                {
                                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] Dashboard wrapper failed: {ex.Message} - Inner: {ex.InnerException?.Message}\n");
                                }
                                catch { }

                                // Fallback: create a simple ViewModel that exposes the backend service data
                                view.DataContext = CreateDashboardFallback(_backendService);
                                Console.WriteLine($"✅ Fallback DataContext type: {view.DataContext.GetType().Name}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Backend service not available, creating simple dashboard");

                            try
                            {
                                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                                File.AppendAllText(logPath, $"[{DateTime.Now}] Backend service not available - using fallback\n");
                            }
                            catch { }

                            // Create a simple dashboard without backend dependency but with reference to check service
                            view.DataContext = new SimpleDashboardViewModel(this);
                            Console.WriteLine($"✅ SimpleDashboardViewModel DataContext type: {view.DataContext.GetType().Name}");
                            Console.WriteLine($"✅ SimpleDashboardViewModel CollectionStatusText: {((SimpleDashboardViewModel)view.DataContext).CollectionStatusText}");
                        }
                        break;
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
            private readonly MainWindow? _mainWindow;

            public SimpleDashboardViewModel(MainWindow? mainWindow = null)
            {
                _mainWindow = mainWindow;
            }

            public string CollectionStatusText => $"🔴 Backend service status: {(_mainWindow?.BackendService != null ? "Available but not connected" : "NULL")}";
            public string DataSourcesSummary => "Services temporarily using fallback - click Refresh to reconnect";
            public bool IsCollectionRunning => false;
            public ObservableCollection<object> RealtimeData => new ObservableCollection<object>();
            public ObservableCollection<object> RecentActivities => new ObservableCollection<object>();
            public SimpleDashboardMetrics Metrics => new SimpleDashboardMetrics();

            public RelayCommand StartCollectionCommand => new RelayCommand(() =>
            {
                Console.WriteLine("⚠️ Start Collection using fallback - attempting to reconnect to backend services");
                if (_mainWindow?.BackendService != null)
                {
                    MessageBox.Show("Backend service is available! Dashboard needs to be refreshed. Try clicking Dashboard again.",
                                  "Service Available", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Backend service is not initialized. Check console for initialization errors.",
                                  "Service Unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });

            public RelayCommand StopCollectionCommand => new RelayCommand(() =>
            {
                Console.WriteLine("⚠️ Stop Collection disabled - backend services not connected");
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
        public class DashboardViewModelWrapper : INotifyPropertyChanged
        {
            private readonly IntegratedBackendService _backendService;

            public DashboardViewModelWrapper(IntegratedBackendService backendService)
            {
                _backendService = backendService;

                // Subscribe to data changes to update UI
                _backendService.RealtimeData.CollectionChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(RecentDataPoints));
                    OnPropertyChanged(nameof(ChartData));
                };

                // Log detailed information about the backend service
                Console.WriteLine("✅ DashboardViewModelWrapper created successfully");
                Console.WriteLine($"Backend service type: {backendService?.GetType().Name}");
                Console.WriteLine($"Backend service status: {backendService?.Status}");
                Console.WriteLine($"Backend service connected: {backendService?.IsConnected}");
                Console.WriteLine($"🔍 CollectionStatusText will be: '{CollectionStatusText}'");
                Console.WriteLine($"🔍 DataSourcesSummary will be: '{DataSourcesSummary}'");

                try
                {
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "backend_init.log");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] DashboardViewModelWrapper created with backend service: {backendService?.GetType().Name}, Status: {backendService?.Status}, Connected: {backendService?.IsConnected}\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] CollectionStatusText: '{CollectionStatusText}', DataSourcesSummary: '{DataSourcesSummary}'\n");
                }
                catch { }
            }

            public string CollectionStatusText => _backendService.IsConnected ? "🟢 Live Data Active" : "🔴 Disconnected";
            public string DataSourcesSummary => $"Status: {_backendService.Status}";
            public bool IsCollectionRunning => _backendService.IsConnected;
            public ObservableCollection<DataPoint> RealtimeData => _backendService.RealtimeData;
            public ObservableCollection<ActivityEvent> RecentActivities => _backendService.RecentActivities;
            public DashboardMetrics Metrics => _backendService.Metrics;

            // Property for Dashboard XAML binding - shows latest data points
            public ObservableCollection<DataPoint> RecentDataPoints => _backendService.RealtimeData;

            // Property for simple chart data - shows recent values as chart points
            public ObservableCollection<object> ChartData
            {
                get
                {
                    var chartPoints = new ObservableCollection<object>();
                    var recentData = _backendService.RealtimeData.TakeLast(10).ToList();

                    foreach (var point in recentData)
                    {
                        chartPoints.Add(new
                        {
                            Label = point.TagName?.Substring(point.TagName.Length - 3) ?? "?",
                            Value = Math.Min(Convert.ToDouble(point.Value) / 10, 130) // Scale for chart height
                        });
                    }

                    return chartPoints;
                }
            }

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

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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

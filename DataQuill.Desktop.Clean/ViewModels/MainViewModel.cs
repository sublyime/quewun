using DataQuillDesktop.Commands;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;
using System.Windows.Input;

namespace DataQuillDesktop.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IntegratedBackendService _backendService;
    private string _currentSection = "Dashboard";
    private bool _isConnected = false;
    private string _connectionStatus = "Disconnected";
    private string _welcomeMessage = "Welcome to DataQuill Desktop";

    public MainViewModel(IntegratedBackendService backendService)
    {
        _backendService = backendService;

        // Initialize commands
        NavigateCommand = new RelayCommand<string>(Navigate);
        TestConnectionCommand = new RelayCommand(async () => await TestConnection());

        // Wire up backend service events
        _backendService.ServiceStatusChanged += OnServiceStatusChanged;
        _backendService.DataPointReceived += OnDataPointReceived;
        _backendService.ActivityOccurred += OnActivityOccurred;

        // Start backend services on startup
        _ = StartBackendServices();
    }

    // Constructor that accepts IApiService for backward compatibility
    public MainViewModel(IApiService apiService)
    {
        // Create integrated backend service and use it instead
        _backendService = new IntegratedBackendService();

        // Initialize commands
        NavigateCommand = new RelayCommand<string>(Navigate);
        TestConnectionCommand = new RelayCommand(async () => await TestConnection());

        // Wire up backend service events
        _backendService.ServiceStatusChanged += OnServiceStatusChanged;
        _backendService.DataPointReceived += OnDataPointReceived;
        _backendService.ActivityOccurred += OnActivityOccurred;

        // Start backend services on startup
        _ = StartBackendServices();
    }

    public string CurrentSection
    {
        get => _currentSection;
        set => SetProperty(ref _currentSection, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    public ICommand NavigateCommand { get; }
    public ICommand TestConnectionCommand { get; }

    private void Navigate(string? section)
    {
        if (!string.IsNullOrEmpty(section))
        {
            CurrentSection = section;
            Console.WriteLine($"Navigated to: {section}");

            // Load cloud connections when navigating to terminal
            if (section == "Terminal")
            {
                // Get the terminal view model and load cloud connections
                var terminalViewModel = new TerminalViewModel();
                terminalViewModel.LoadCloudConnections();
            }
        }
    }

    private async Task TestConnection()
    {
        try
        {
            IsConnected = await _backendService.HealthCheckAsync();
            ConnectionStatus = IsConnected ? "Connected to Backend Services" : "Backend Services Unavailable";

            if (IsConnected)
            {
                WelcomeMessage = "DataQuill Desktop - All Services Running!";
                Console.WriteLine("✅ Backend services connection successful");
            }
            else
            {
                WelcomeMessage = "DataQuill Desktop - Services Starting...";
                Console.WriteLine("⚠️ Backend services not ready");
            }
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = "Connection Error";
            WelcomeMessage = "DataQuill Desktop - Service Error";
            Console.WriteLine($"❌ Connection test failed: {ex.Message}");
        }
    }

    private async Task StartBackendServices()
    {
        try
        {
            await _backendService.StartAsync();
            Console.WriteLine("✅ Integrated backend services started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error starting backend services: {ex.Message}");
        }
    }

    private void OnServiceStatusChanged(object? sender, bool isRunning)
    {
        IsConnected = isRunning;
        ConnectionStatus = isRunning ? "Backend Services Running" : "Backend Services Stopped";
        WelcomeMessage = isRunning ? "DataQuill Desktop - All Services Running!" : "DataQuill Desktop - Services Stopped";
    }

    private void OnDataPointReceived(object? sender, DataQuillDesktop.Models.DataPoint dataPoint)
    {
        Console.WriteLine($"📊 Data point received: {dataPoint.Value} from {dataPoint.TagName}");
    }

    private void OnActivityOccurred(object? sender, DataQuillDesktop.Models.ActivityEvent activity)
    {
        Console.WriteLine($"📝 Activity: {activity.Title} - {activity.Description}");
    }
}
using DataQuillDesktop.Commands;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;
using System.Windows.Input;

namespace DataQuillDesktop.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private string _currentSection = "Dashboard";
    private bool _isConnected = false;
    private string _connectionStatus = "Disconnected";
    private string _welcomeMessage = "Welcome to DataQuill Desktop";

    public MainViewModel(IApiService apiService)
    {
        _apiService = apiService;

        // Initialize commands
        NavigateCommand = new RelayCommand<string>(Navigate);
        TestConnectionCommand = new RelayCommand(async () => await TestConnection());

        // Test connection on startup
        _ = TestConnection();
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
        }
    }

    private async Task TestConnection()
    {
        try
        {
            IsConnected = await _apiService.TestConnectionAsync();
            ConnectionStatus = IsConnected ? "Connected to Backend" : "Backend Unavailable";

            if (IsConnected)
            {
                WelcomeMessage = "DataQuill Desktop - Connected and Ready!";
                Console.WriteLine("✅ Backend connection successful");
            }
            else
            {
                WelcomeMessage = "DataQuill Desktop - Working Offline";
                Console.WriteLine("⚠️ Backend connection failed - working offline");
            }
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = "Connection Error";
            WelcomeMessage = "DataQuill Desktop - Connection Error";
            Console.WriteLine($"❌ Connection test failed: {ex.Message}");
        }
    }
}
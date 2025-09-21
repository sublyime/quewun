using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;
using DataQuillDesktop.Commands;

namespace DataQuillDesktop
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            try
            {
                Console.WriteLine("=== DataQuill Desktop Starting ===");
                InitializeComponent();
                Console.WriteLine("XAML loaded successfully");

                // Initialize the MainViewModel with error handling
                try
                {
                    var apiService = new ApiService();
                    Console.WriteLine("ApiService created");

                    _viewModel = new MainViewModel(apiService);
                    Console.WriteLine("MainViewModel created");

                    DataContext = _viewModel;
                    Console.WriteLine("DataContext set to MainViewModel");
                }
                catch (Exception vmEx)
                {
                    Console.WriteLine($"Error creating ViewModel: {vmEx.Message}");
                    Console.WriteLine($"Stack trace: {vmEx.StackTrace}");

                    // Create a working fallback view model with navigation
                    DataContext = new FallbackViewModel();
                    Console.WriteLine("Using fallback DataContext with navigation");
                }

                Console.WriteLine("Window initialized successfully!");
                Console.WriteLine("=== DataQuill Desktop Ready ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during initialization: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Last resort fallback
                DataContext = new FallbackViewModel();
                Console.WriteLine("Using emergency fallback");
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
            if (!string.IsNullOrEmpty(section))
            {
                CurrentSection = section;
                Console.WriteLine($"Fallback navigation to: {section}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

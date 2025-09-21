using DataQuillDesktop.Models;
using DataQuillDesktop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DataQuillDesktop.Commands;

namespace DataQuillDesktop.ViewModels;

/// <summary>
/// ViewModel for the Dashboard view, managing real-time data display and metrics
/// </summary>
public class DashboardViewModel : BaseViewModel, IDisposable
{
    private readonly DataCollectionService _dataCollectionService;
    private bool _disposed = false;

    // Data collections for binding
    public ObservableCollection<DataPoint> RealtimeData => _dataCollectionService.RealtimeData;
    public ObservableCollection<ActivityEvent> RecentActivities => _dataCollectionService.RecentActivities;
    public DashboardMetrics Metrics => _dataCollectionService.Metrics;

    // Chart data for visualization
    public ObservableCollection<ChartDataPoint> ChartData { get; } = new();
    
    // Commands
    public ICommand StartCollectionCommand { get; }
    public ICommand StopCollectionCommand { get; }
    public ICommand RefreshCommand { get; }

    private bool _isCollectionRunning;
    public bool IsCollectionRunning
    {
        get => _isCollectionRunning;
        set
        {
            _isCollectionRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CollectionStatusText));
            OnPropertyChanged(nameof(CollectionStatusColor));
        }
    }

    public string CollectionStatusText => IsCollectionRunning ? "🟢 Active" : "🔴 Stopped";
    public string CollectionStatusColor => IsCollectionRunning ? "Green" : "Red";

    // Recent data points for quick display
    private ObservableCollection<DataPoint> _recentDataPoints = new();
    public ObservableCollection<DataPoint> RecentDataPoints
    {
        get => _recentDataPoints;
        set
        {
            _recentDataPoints = value;
            OnPropertyChanged();
        }
    }

    // Summary statistics
    private string _dataSourcesSummary = "No data sources configured";
    public string DataSourcesSummary
    {
        get => _dataSourcesSummary;
        set
        {
            _dataSourcesSummary = value;
            OnPropertyChanged();
        }
    }

    public DashboardViewModel(DataCollectionService dataCollectionService)
    {
        _dataCollectionService = dataCollectionService;
        
        // Initialize commands
        StartCollectionCommand = new RelayCommand(StartDataCollection, () => !IsCollectionRunning);
        StopCollectionCommand = new RelayCommand(StopDataCollection, () => IsCollectionRunning);
        RefreshCommand = new RelayCommand(RefreshData);

        // Subscribe to data collection events
        _dataCollectionService.DataPointReceived += OnDataPointReceived;
        _dataCollectionService.ActivityOccurred += OnActivityOccurred;

        // Initialize chart data
        InitializeChartData();
        
        // Start data collection automatically
        StartDataCollection();
    }

    /// <summary>
    /// Start the data collection service
    /// </summary>
    private void StartDataCollection()
    {
        try
        {
            _dataCollectionService.Start();
            IsCollectionRunning = true;
            UpdateDataSourcesSummary();
        }
        catch (Exception ex)
        {
            // Handle error - in a real app, show user notification
            System.Diagnostics.Debug.WriteLine($"Error starting data collection: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the data collection service
    /// </summary>
    private void StopDataCollection()
    {
        try
        {
            _dataCollectionService.Stop();
            IsCollectionRunning = false;
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error stopping data collection: {ex.Message}");
        }
    }

    /// <summary>
    /// Refresh dashboard data
    /// </summary>
    private void RefreshData()
    {
        UpdateRecentDataPoints();
        UpdateChartData();
        UpdateDataSourcesSummary();
    }

    /// <summary>
    /// Handle new data point received
    /// </summary>
    private void OnDataPointReceived(object? sender, DataPoint dataPoint)
    {
        // Update recent data points on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateRecentDataPoints();
            UpdateChartData();
        });
    }

    /// <summary>
    /// Handle new activity occurred
    /// </summary>
    private void OnActivityOccurred(object? sender, ActivityEvent activity)
    {
        // Activities are automatically added to the RecentActivities collection
        // UI will update automatically due to ObservableCollection binding
    }

    /// <summary>
    /// Update the recent data points display
    /// </summary>
    private void UpdateRecentDataPoints()
    {
        var recent = RealtimeData
            .OrderByDescending(dp => dp.Timestamp)
            .Take(10)
            .ToList();

        RecentDataPoints.Clear();
        foreach (var dataPoint in recent)
        {
            RecentDataPoints.Add(dataPoint);
        }
    }

    /// <summary>
    /// Update chart data for visualization
    /// </summary>
    private void UpdateChartData()
    {
        // Group recent data by minute and calculate averages
        var chartPoints = RealtimeData
            .Where(dp => dp.Timestamp >= DateTime.Now.AddMinutes(-10))
            .GroupBy(dp => new DateTime(dp.Timestamp.Year, dp.Timestamp.Month, dp.Timestamp.Day, 
                                       dp.Timestamp.Hour, dp.Timestamp.Minute, 0))
            .Select(g => new ChartDataPoint
            {
                Timestamp = g.Key,
                Value = g.Average(dp => dp.NumericValue),
                Label = g.Key.ToString("HH:mm")
            })
            .OrderBy(cp => cp.Timestamp)
            .ToList();

        ChartData.Clear();
        foreach (var point in chartPoints)
        {
            ChartData.Add(point);
        }
    }

    /// <summary>
    /// Update data sources summary
    /// </summary>
    private void UpdateDataSourcesSummary()
    {
        var activeCount = Metrics.ActiveConnections;
        var totalPoints = Metrics.TotalDataPoints;
        
        DataSourcesSummary = activeCount > 0 
            ? $"{activeCount} active connections • {totalPoints} data points collected"
            : "No active data sources";
    }

    /// <summary>
    /// Initialize chart data with sample points
    /// </summary>
    private void InitializeChartData()
    {
        var now = DateTime.Now;
        for (int i = 9; i >= 0; i--)
        {
            ChartData.Add(new ChartDataPoint
            {
                Timestamp = now.AddMinutes(-i),
                Value = new Random().NextDouble() * 100,
                Label = now.AddMinutes(-i).ToString("HH:mm")
            });
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_dataCollectionService != null)
            {
                _dataCollectionService.DataPointReceived -= OnDataPointReceived;
                _dataCollectionService.ActivityOccurred -= OnActivityOccurred;
                _dataCollectionService.Dispose();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a data point for chart visualization
/// </summary>
public class ChartDataPoint : INotifyPropertyChanged
{
    private DateTime _timestamp;
    private double _value;
    private string _label = string.Empty;

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            _timestamp = value;
            OnPropertyChanged(nameof(Timestamp));
        }
    }

    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            _label = value;
            OnPropertyChanged(nameof(Label));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
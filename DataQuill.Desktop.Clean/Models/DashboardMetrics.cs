using System.ComponentModel;

namespace DataQuillDesktop.Models;

/// <summary>
/// Represents real-time statistics and metrics for the dashboard
/// </summary>
public class DashboardMetrics : INotifyPropertyChanged
{
    private int _activeConnections;
    private int _totalDataPoints;
    private long _dataProcessedToday;
    private int _activeUsers;
    private double _averageResponseTime;
    private DateTime _lastUpdate;

    public int ActiveConnections
    {
        get => _activeConnections;
        set
        {
            _activeConnections = value;
            OnPropertyChanged(nameof(ActiveConnections));
        }
    }

    public int TotalDataPoints
    {
        get => _totalDataPoints;
        set
        {
            _totalDataPoints = value;
            OnPropertyChanged(nameof(TotalDataPoints));
        }
    }

    public long DataProcessedToday
    {
        get => _dataProcessedToday;
        set
        {
            _dataProcessedToday = value;
            OnPropertyChanged(nameof(DataProcessedToday));
            OnPropertyChanged(nameof(DataProcessedTodayFormatted));
        }
    }

    public int ActiveUsers
    {
        get => _activeUsers;
        set
        {
            _activeUsers = value;
            OnPropertyChanged(nameof(ActiveUsers));
        }
    }

    public double AverageResponseTime
    {
        get => _averageResponseTime;
        set
        {
            _averageResponseTime = value;
            OnPropertyChanged(nameof(AverageResponseTime));
            OnPropertyChanged(nameof(AverageResponseTimeFormatted));
        }
    }

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
            OnPropertyChanged(nameof(LastUpdate));
            OnPropertyChanged(nameof(LastUpdateFormatted));
        }
    }

    // Formatted properties for display
    public string DataProcessedTodayFormatted
    {
        get
        {
            if (DataProcessedToday < 1024) return $"{DataProcessedToday} B";
            if (DataProcessedToday < 1024 * 1024) return $"{DataProcessedToday / 1024.0:F1} KB";
            if (DataProcessedToday < 1024 * 1024 * 1024) return $"{DataProcessedToday / (1024.0 * 1024.0):F1} MB";
            return $"{DataProcessedToday / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    public string AverageResponseTimeFormatted => $"{AverageResponseTime:F0}ms";

    public string LastUpdateFormatted => LastUpdate.ToString("HH:mm:ss");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
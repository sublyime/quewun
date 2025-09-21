using System.ComponentModel;

namespace DataQuillDesktop.Models;

/// <summary>
/// Represents a real-time activity event for the dashboard
/// </summary>
public class ActivityEvent : INotifyPropertyChanged
{
    private DateTime _timestamp;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private ActivityType _type;

    public int Id { get; set; }

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            _timestamp = value;
            OnPropertyChanged(nameof(Timestamp));
            OnPropertyChanged(nameof(TimeAgo));
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    public ActivityType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(TypeIcon));
            OnPropertyChanged(nameof(TypeColor));
        }
    }

    // Display properties
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - Timestamp;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            return $"{(int)timeSpan.TotalDays}d ago";
        }
    }

    public string TypeIcon => Type switch
    {
        ActivityType.Connection => "🔗",
        ActivityType.DataReceived => "📊",
        ActivityType.Error => "⚠️",
        ActivityType.Warning => "⚡",
        ActivityType.Success => "✅",
        ActivityType.Info => "ℹ️",
        _ => "📄"
    };

    public string TypeColor => Type switch
    {
        ActivityType.Connection => "LightBlue",
        ActivityType.DataReceived => "LightGreen",
        ActivityType.Error => "LightCoral",
        ActivityType.Warning => "LightYellow",
        ActivityType.Success => "LightGreen",
        ActivityType.Info => "LightGray",
        _ => "White"
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Types of activity events
/// </summary>
public enum ActivityType
{
    Connection,
    DataReceived,
    Error,
    Warning,
    Success,
    Info
}
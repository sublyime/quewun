using System.ComponentModel;

namespace DataQuillDesktop.Models;

/// <summary>
/// Represents a real-time data point from a data source
/// </summary>
public class DataPoint : INotifyPropertyChanged
{
    private DateTime _timestamp;
    private object? _value;
    private string _quality = "Good";

    public int Id { get; set; }
    public int DataSourceId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string DataType { get; set; } = "Unknown";
    public string Unit { get; set; } = string.Empty;

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            _timestamp = value;
            OnPropertyChanged(nameof(Timestamp));
        }
    }

    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public string Quality
    {
        get => _quality;
        set
        {
            _quality = value;
            OnPropertyChanged(nameof(Quality));
        }
    }

    // Convenience properties for data binding
    public double NumericValue
    {
        get
        {
            if (Value == null) return 0;
            if (double.TryParse(Value.ToString(), out double result))
                return result;
            return 0;
        }
    }

    public string DisplayValue
    {
        get
        {
            if (Value == null) return "N/A";
            return $"{Value} {Unit}".Trim();
        }
    }

    public bool IsGoodQuality => Quality == "Good";

    // Navigation property
    public virtual DataSource? DataSource { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
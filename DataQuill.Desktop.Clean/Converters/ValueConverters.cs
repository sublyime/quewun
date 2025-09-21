using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DataQuillDesktop.Converters;

public class SectionToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentSection && parameter is string targetSection)
        {
            return currentSection == targetSection ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colors)
        {
            var colorPair = colors.Split(';');
            if (colorPair.Length == 2)
            {
                var color = boolValue ? colorPair[0] : colorPair[1];
                return (Color)ColorConverter.ConvertFromString(color);
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string options)
        {
            var optionPair = options.Split(';');
            if (optionPair.Length == 2)
            {
                return boolValue ? optionPair[0] : optionPair[1];
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MessageTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string messageType)
        {
            return messageType?.ToLower() switch
            {
                "error" => new SolidColorBrush(Colors.Red),
                "warning" => new SolidColorBrush(Colors.Orange),
                "info" => new SolidColorBrush(Colors.Blue),
                "success" => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }
        return true;
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AddEditHeaderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? "Add New Connection" : "Edit Connection";
        }
        return "Edit Connection";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return "#E3F2FD";
        }
        return "Transparent";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts terminal connection status to color
/// </summary>
public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataQuillDesktop.Models.Terminal.TerminalConnectionStatus status)
        {
            return status switch
            {
                DataQuillDesktop.Models.Terminal.TerminalConnectionStatus.Connected => "#4CAF50",
                DataQuillDesktop.Models.Terminal.TerminalConnectionStatus.Connecting => "#FF9800",
                DataQuillDesktop.Models.Terminal.TerminalConnectionStatus.ConnectionFailed => "#F44336",
                DataQuillDesktop.Models.Terminal.TerminalConnectionStatus.Disconnected => "#757575",
                _ => "#757575"
            };
        }
        return "#757575";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts terminal message type to color
/// </summary>
public class MessageTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataQuillDesktop.Services.TerminalMessageType messageType)
        {
            return messageType switch
            {
                DataQuillDesktop.Services.TerminalMessageType.Received => "#00FF00",  // Green
                DataQuillDesktop.Services.TerminalMessageType.Sent => "#FFFF00",      // Yellow
                DataQuillDesktop.Services.TerminalMessageType.System => "#00FFFF",    // Cyan
                DataQuillDesktop.Services.TerminalMessageType.Error => "#FF0000",     // Red
                DataQuillDesktop.Services.TerminalMessageType.Warning => "#FFA500",  // Orange
                DataQuillDesktop.Services.TerminalMessageType.Info => "#FFFFFF",     // White
                _ => "#FFFFFF"
            };
        }
        return "#FFFFFF";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts enum values to visibility for conditional UI display
/// </summary>
public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return Visibility.Collapsed;

        string enumValue = value.ToString() ?? string.Empty;
        string targetValue = parameter.ToString() ?? string.Empty;

        return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
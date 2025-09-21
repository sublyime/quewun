using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataQuillDesktop.Models.Terminal
{
    /// <summary>
    /// Base class for terminal connections
    /// </summary>
    public abstract class TerminalConnectionBase : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private string _description = string.Empty;
        private TerminalConnectionType _connectionType;
        private TerminalConnectionStatus _status = TerminalConnectionStatus.Disconnected;
        private DateTime _lastConnected;
        private string _errorMessage = string.Empty;
        private TerminalEmulationType _emulationType = TerminalEmulationType.VT100;
        private int _terminalWidth = 80;
        private int _terminalHeight = 24;
        private bool _autoReconnect = false;
        private string _sessionData = string.Empty;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public TerminalConnectionType ConnectionType
        {
            get => _connectionType;
            set => SetProperty(ref _connectionType, value);
        }

        public TerminalConnectionStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime LastConnected
        {
            get => _lastConnected;
            set => SetProperty(ref _lastConnected, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public TerminalEmulationType EmulationType
        {
            get => _emulationType;
            set => SetProperty(ref _emulationType, value);
        }

        public int TerminalWidth
        {
            get => _terminalWidth;
            set => SetProperty(ref _terminalWidth, value);
        }

        public int TerminalHeight
        {
            get => _terminalHeight;
            set => SetProperty(ref _terminalHeight, value);
        }

        public bool AutoReconnect
        {
            get => _autoReconnect;
            set => SetProperty(ref _autoReconnect, value);
        }

        public string SessionData
        {
            get => _sessionData;
            set => SetProperty(ref _sessionData, value);
        }

        /// <summary>
        /// Validates the connection configuration
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// Gets the connection identifier for display
        /// </summary>
        public abstract string GetConnectionIdentifier();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
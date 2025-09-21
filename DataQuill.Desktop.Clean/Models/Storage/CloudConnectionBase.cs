using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataQuillDesktop.Models.Storage
{
    /// <summary>
    /// Base class for cloud connection configurations
    /// </summary>
    public abstract class CloudConnectionBase : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private string _description = string.Empty;
        private CloudProvider _provider;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private DateTime _lastConnected;
        private string _errorMessage = string.Empty;

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

        public CloudProvider Provider
        {
            get => _provider;
            set => SetProperty(ref _provider, value);
        }

        public ConnectionStatus Status
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

        /// <summary>
        /// Gets the authentication type required for this provider
        /// </summary>
        public abstract AuthenticationType AuthType { get; }

        /// <summary>
        /// Validates the connection configuration
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// Gets the connection string or identifier for this connection
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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataQuillDesktop.Commands;
using DataQuillDesktop.Models.Storage;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;

namespace DataQuillDesktop.ViewModels
{
    public class StorageViewModel : BaseViewModel
    {
        private readonly CloudConnectionManager _connectionManager;
        private CloudConnectionBase? _selectedConnection;
        private CloudConnectionBase? _editingConnection;
        private CloudProvider _selectedProvider = CloudProvider.AWS;
        private bool _isAddingConnection = false;
        private bool _isEditingConnection = false;
        private string _searchText = string.Empty;

        public StorageViewModel()
        {
            _connectionManager = CloudConnectionManager.Instance;

            // Initialize commands
            AddConnectionCommand = new RelayCommand(StartAddConnection);
            EditConnectionCommand = new RelayCommand<CloudConnectionBase>(StartEditConnection, CanEditConnection);
            DeleteConnectionCommand = new RelayCommand<CloudConnectionBase>(DeleteConnection, CanDeleteConnection);
            SaveConnectionCommand = new RelayCommand(SaveConnection, CanSaveConnection);
            CancelEditCommand = new RelayCommand(CancelEdit);
            TestConnectionCommand = new RelayCommand<CloudConnectionBase>(TestConnection, CanTestConnection);
            ConnectCommand = new RelayCommand<CloudConnectionBase>(Connect, CanConnect);
            DisconnectCommand = new RelayCommand<CloudConnectionBase>(Disconnect, CanDisconnect);
            RefreshCommand = new RelayCommand(Refresh);

            // Load connections
            Refresh();
        }

        #region Properties

        public ObservableCollection<CloudConnectionBase> Connections => _connectionManager.Connections;

        public ObservableCollection<CloudConnectionBase> FilteredConnections
        {
            get
            {
                var filtered = Connections.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(c =>
                        c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        c.Provider.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                return new ObservableCollection<CloudConnectionBase>(filtered);
            }
        }

        public CloudConnectionBase? SelectedConnection
        {
            get => _selectedConnection;
            set => SetProperty(ref _selectedConnection, value);
        }

        public CloudConnectionBase? EditingConnection
        {
            get => _editingConnection;
            set => SetProperty(ref _editingConnection, value);
        }

        public CloudProvider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value);
        }

        public Array CloudProviders => Enum.GetValues<CloudProvider>();

        public bool IsAddingConnection
        {
            get => _isAddingConnection;
            set => SetProperty(ref _isAddingConnection, value);
        }

        public bool IsEditingConnection
        {
            get => _isEditingConnection;
            set => SetProperty(ref _isEditingConnection, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(FilteredConnections));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddConnectionCommand { get; }
        public ICommand EditConnectionCommand { get; }
        public ICommand DeleteConnectionCommand { get; }
        public ICommand SaveConnectionCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Command Implementations

        private void StartAddConnection()
        {
            EditingConnection = _connectionManager.CreateConnection(SelectedProvider);
            IsAddingConnection = true;
            IsEditingConnection = true;
        }

        private void StartEditConnection(CloudConnectionBase? connection)
        {
            if (connection == null) return;

            // Create a copy for editing
            EditingConnection = CloneConnection(connection);
            IsAddingConnection = false;
            IsEditingConnection = true;
        }

        private bool CanEditConnection(CloudConnectionBase? connection)
        {
            return connection != null && !IsEditingConnection;
        }

        private void DeleteConnection(CloudConnectionBase? connection)
        {
            if (connection == null) return;

            _connectionManager.RemoveConnection(connection.Id);
            OnPropertyChanged(nameof(FilteredConnections));
        }

        private bool CanDeleteConnection(CloudConnectionBase? connection)
        {
            return connection != null && !IsEditingConnection;
        }

        private void SaveConnection()
        {
            if (EditingConnection == null) return;

            try
            {
                if (IsAddingConnection)
                {
                    _connectionManager.AddConnection(EditingConnection);
                }
                else
                {
                    _connectionManager.UpdateConnection(EditingConnection);
                }

                CancelEdit();
                OnPropertyChanged(nameof(FilteredConnections));
            }
            catch (Exception ex)
            {
                // Handle error (in a real app, you might show a message box or toast)
                Console.WriteLine($"Error saving connection: {ex.Message}");
            }
        }

        private bool CanSaveConnection()
        {
            return EditingConnection?.IsValid() == true;
        }

        private void CancelEdit()
        {
            EditingConnection = null;
            IsAddingConnection = false;
            IsEditingConnection = false;
        }

        private async void TestConnection(CloudConnectionBase? connection)
        {
            if (connection == null) return;

            try
            {
                var (success, errorMessage) = await _connectionManager.TestConnectionAsync(connection);

                if (!success)
                {
                    // Handle test failure (show message, etc.)
                    Console.WriteLine($"Connection test failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing connection: {ex.Message}");
            }
        }

        private bool CanTestConnection(CloudConnectionBase? connection)
        {
            return connection?.IsValid() == true && connection.Status != ConnectionStatus.Testing;
        }

        private async void Connect(CloudConnectionBase? connection)
        {
            if (connection == null) return;

            try
            {
                await _connectionManager.ConnectAsync(connection.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting: {ex.Message}");
            }
        }

        private bool CanConnect(CloudConnectionBase? connection)
        {
            return connection?.Status == ConnectionStatus.Disconnected;
        }

        private void Disconnect(CloudConnectionBase? connection)
        {
            if (connection == null) return;

            _connectionManager.Disconnect(connection.Id);
        }

        private bool CanDisconnect(CloudConnectionBase? connection)
        {
            return connection?.Status == ConnectionStatus.Connected;
        }

        private void Refresh()
        {
            OnPropertyChanged(nameof(Connections));
            OnPropertyChanged(nameof(FilteredConnections));
        }

        #endregion

        #region Helper Methods

        private CloudConnectionBase CloneConnection(CloudConnectionBase original)
        {
            return original.Provider switch
            {
                CloudProvider.AWS => CloneAwsConnection((AwsConnection)original),
                CloudProvider.GoogleCloud => CloneGoogleCloudConnection((GoogleCloudConnection)original),
                CloudProvider.Azure => CloneAzureConnection((AzureConnection)original),
                CloudProvider.Oracle => CloneOracleConnection((OracleConnection)original),
                _ => throw new ArgumentException($"Unsupported provider: {original.Provider}")
            };
        }

        private AwsConnection CloneAwsConnection(AwsConnection original)
        {
            return new AwsConnection
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                AccessKeyId = original.AccessKeyId,
                SecretAccessKey = original.SecretAccessKey,
                Region = original.Region,
                SessionToken = original.SessionToken,
                UseSessionToken = original.UseSessionToken,
                Status = original.Status,
                LastConnected = original.LastConnected,
                ErrorMessage = original.ErrorMessage
            };
        }

        private GoogleCloudConnection CloneGoogleCloudConnection(GoogleCloudConnection original)
        {
            return new GoogleCloudConnection
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                ProjectId = original.ProjectId,
                ServiceAccountJson = original.ServiceAccountJson,
                KeyFilePath = original.KeyFilePath,
                UseKeyFile = original.UseKeyFile,
                Status = original.Status,
                LastConnected = original.LastConnected,
                ErrorMessage = original.ErrorMessage
            };
        }

        private AzureConnection CloneAzureConnection(AzureConnection original)
        {
            return new AzureConnection
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                ConnectionString = original.ConnectionString,
                TenantId = original.TenantId,
                ClientId = original.ClientId,
                ClientSecret = original.ClientSecret,
                SubscriptionId = original.SubscriptionId,
                UseManagedIdentity = original.UseManagedIdentity,
                Status = original.Status,
                LastConnected = original.LastConnected,
                ErrorMessage = original.ErrorMessage
            };
        }

        private OracleConnection CloneOracleConnection(OracleConnection original)
        {
            return new OracleConnection
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                UserId = original.UserId,
                Fingerprint = original.Fingerprint,
                TenancyId = original.TenancyId,
                Region = original.Region,
                PrivateKeyPath = original.PrivateKeyPath,
                PrivateKeyContent = original.PrivateKeyContent,
                UseKeyFile = original.UseKeyFile,
                Status = original.Status,
                LastConnected = original.LastConnected,
                ErrorMessage = original.ErrorMessage
            };
        }

        #endregion
    }
}
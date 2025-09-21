using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataQuillDesktop.Commands;
using DataQuillDesktop.Models.Terminal;
using DataQuillDesktop.Services;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace DataQuillDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Terminal section
    /// </summary>
    public class TerminalViewModel : INotifyPropertyChanged
    {
        private readonly TerminalConnectionManager _connectionManager;
        private TerminalConnectionBase? _selectedConnection;
        private TerminalSession? _selectedSession;
        private TerminalConnectionType _selectedConnectionType;
        private bool _isConnecting;
        private string _inputCommand = string.Empty;

        public TerminalViewModel()
        {
            _connectionManager = TerminalConnectionManager.Instance;

            // Initialize commands
            AddConnectionCommand = new RelayCommand(AddConnection);
            RemoveConnectionCommand = new RelayCommand(RemoveConnection, () => SelectedConnection != null);
            EditConnectionCommand = new RelayCommand(EditConnection, () => SelectedConnection != null);
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => SelectedConnection != null && !IsConnecting);
            DisconnectCommand = new RelayCommand(DisconnectSession, () => SelectedSession?.IsConnected == true);
            SendCommandCommand = new RelayCommand(async () => await SendCommandAsync(), () => SelectedSession?.IsConnected == true && !string.IsNullOrWhiteSpace(InputCommand));
            ClearTerminalCommand = new RelayCommand(ClearTerminal, () => SelectedSession != null);

            // Set default connection type
            SelectedConnectionType = TerminalConnectionType.SSH;
        }

        public ObservableCollection<TerminalConnectionBase> Connections => _connectionManager.Connections;
        public ObservableCollection<TerminalSession> ActiveSessions => _connectionManager.ActiveSessions;

        public TerminalConnectionBase? SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                if (_selectedConnection != value)
                {
                    _selectedConnection = value;
                    OnPropertyChanged();

                    // Update selected session if connection has an active session
                    if (value != null)
                    {
                        SelectedSession = _connectionManager.GetActiveSession(value.Id);
                    }

                    // Update command states
                    ((RelayCommand)RemoveConnectionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)EditConnectionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public TerminalSession? SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (_selectedSession != value)
                {
                    _selectedSession = value;
                    OnPropertyChanged();

                    // Update command states
                    ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)SendCommandCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ClearTerminalCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public TerminalConnectionType SelectedConnectionType
        {
            get => _selectedConnectionType;
            set
            {
                if (_selectedConnectionType != value)
                {
                    _selectedConnectionType = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if (_isConnecting != value)
                {
                    _isConnecting = value;
                    OnPropertyChanged();
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string InputCommand
        {
            get => _inputCommand;
            set
            {
                if (_inputCommand != value)
                {
                    _inputCommand = value;
                    OnPropertyChanged();
                    ((RelayCommand)SendCommandCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // Available connection types for UI binding
        public Array ConnectionTypes => Enum.GetValues(typeof(TerminalConnectionType));

        // Available emulation types for UI binding
        public Array EmulationTypes => Enum.GetValues(typeof(TerminalEmulationType));

        // Available authentication types for SSH
        public Array AuthenticationTypes => Enum.GetValues(typeof(TerminalAuthType));

        // Cloud connections for data stream monitoring
        public ObservableCollection<CloudConnectionInfo> CloudConnections { get; } = new();

        // Commands
        public ICommand AddConnectionCommand { get; }
        public ICommand RemoveConnectionCommand { get; }
        public ICommand EditConnectionCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendCommandCommand { get; }
        public ICommand ClearTerminalCommand { get; }

        /// <summary>
        /// Adds a new connection
        /// </summary>
        private void AddConnection()
        {
            try
            {
                var newConnection = _connectionManager.CreateConnection(SelectedConnectionType);

                // Ensure we're on the UI thread when modifying collections
                if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true)
                {
                    _connectionManager.AddConnection(newConnection);
                    SelectedConnection = newConnection;
                }
                else
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        _connectionManager.AddConnection(newConnection);
                        SelectedConnection = newConnection;
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error to debug output and console
                System.Diagnostics.Debug.WriteLine($"Error adding connection: {ex.Message}");
                System.Console.WriteLine($"Error adding connection: {ex}");

                // Show a message box to the user (ensure it's on UI thread)
                var showError = new Action(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Error adding connection: {ex.Message}",
                        "Connection Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                });

                if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true)
                {
                    showError();
                }
                else
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(showError);
                }
            }
        }

        /// <summary>
        /// Removes the selected connection
        /// </summary>
        private void RemoveConnection()
        {
            if (SelectedConnection == null) return;

            _connectionManager.RemoveConnection(SelectedConnection.Id);
            SelectedConnection = null;
        }

        /// <summary>
        /// Edits the selected connection (placeholder for edit dialog)
        /// </summary>
        private void EditConnection()
        {
            if (SelectedConnection == null) return;

            // TODO: Open edit dialog
            // For now, we'll just trigger property change to refresh UI
            OnPropertyChanged(nameof(SelectedConnection));
        }

        /// <summary>
        /// Connects to the selected terminal
        /// </summary>
        private async Task ConnectAsync()
        {
            if (SelectedConnection == null || IsConnecting) return;

            try
            {
                IsConnecting = true;
                var session = await _connectionManager.OpenSessionAsync(SelectedConnection.Id);
                if (session != null)
                {
                    SelectedSession = session;

                    // Subscribe to session events
                    session.Disconnected += (s, e) =>
                    {
                        if (SelectedSession == session)
                        {
                            SelectedSession = null;
                        }
                    };
                }
            }
            catch (Exception)
            {
                // Error is already handled in the connection manager and session
                // Just ensure UI state is correct
                SelectedSession = null;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        /// <summary>
        /// Disconnects the current session
        /// </summary>
        private void DisconnectSession()
        {
            if (SelectedSession == null) return;

            _connectionManager.CloseSession(SelectedSession.Id);
            SelectedSession = null;
        }

        /// <summary>
        /// Sends a command to the terminal
        /// </summary>
        private async Task SendCommandAsync()
        {
            if (SelectedSession == null || string.IsNullOrWhiteSpace(InputCommand)) return;

            try
            {
                // Add newline if not present
                var command = InputCommand;
                if (!command.EndsWith("\n") && !command.EndsWith("\r\n"))
                {
                    command += "\r\n";
                }

                await SelectedSession.SendDataAsync(command);
                InputCommand = string.Empty; // Clear input after sending
            }
            catch (Exception)
            {
                // Error handling is done in the session
            }
        }

        /// <summary>
        /// Clears the terminal output
        /// </summary>
        private void ClearTerminal()
        {
            if (SelectedSession == null) return;

            SelectedSession.Messages.Clear();
        }

        /// <summary>
        /// Loads cloud connections for data stream monitoring
        /// </summary>
        public void LoadCloudConnections()
        {
            CloudConnections.Clear();
            foreach (var connection in _connectionManager.GetAvailableCloudConnections())
            {
                CloudConnections.Add(connection);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
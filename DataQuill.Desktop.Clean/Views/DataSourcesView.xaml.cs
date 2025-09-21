using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using DataQuillDesktop.Models;
using DataQuillDesktop.Services;
using Microsoft.Win32;

namespace DataQuillDesktop.Views
{
    public partial class DataSourcesView : UserControl
    {
        public ObservableCollection<DataSource> DataSources { get; set; }
        private DataSource? _currentDataSource;
        private readonly DataSourceService _dataSourceService;

        public DataSourcesView()
        {
            InitializeComponent();
            DataSources = new ObservableCollection<DataSource>();
            DataSourcesList.ItemsSource = DataSources;
            _dataSourceService = new DataSourceService();

            InitializeComboBoxes();
            
            // Load data sources asynchronously after the UI is initialized
            Loaded += async (sender, e) => await LoadDataSources();
        }

        private void InitializeComboBoxes()
        {
            // Initialize Interface ComboBox
            InterfaceComboBox.ItemsSource = Enum.GetValues(typeof(InterfaceType));

            // Initialize Protocol ComboBox
            ProtocolComboBox.ItemsSource = Enum.GetValues(typeof(ProtocolType));

            // Initialize Serial Ports
            RefreshSerialPorts();
        }

        private void RefreshSerialPorts()
        {
            SerialPortComboBox.Items.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                SerialPortComboBox.Items.Add(portName);
            }
        }

        private async Task LoadDataSources()
        {
            try
            {
                DataSources.Clear();
                var dataSources = await _dataSourceService.GetAllDataSourcesAsync();
                foreach (var dataSource in dataSources)
                {
                    DataSources.Add(dataSource);
                }

                // If no data sources exist, add a sample one for testing
                if (DataSources.Count == 0)
                {
                    var sampleDataSource = new DataSource
                    {
                        Id = 1,
                        Name = "Sample Modbus TCP",
                        Description = "Sample Modbus TCP connection",
                        InterfaceType = InterfaceType.TCP,
                        ProtocolType = ProtocolType.ModbusTCP,
                        IsActive = true,
                        Configuration = new DataSourceConfiguration
                        {
                            Host = "127.0.0.1",
                            Port = 502,
                            SlaveId = 1,
                            Timeout = 5
                        }
                    };
                    DataSources.Add(sampleDataSource);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data sources: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataSourcesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataSourcesList.SelectedItem is DataSource selectedDataSource)
            {
                _currentDataSource = selectedDataSource;
                LoadDataSourceConfiguration(selectedDataSource);
                ConfigurationPanel.Visibility = Visibility.Visible;
                WelcomePanel.Visibility = Visibility.Collapsed;
                EditDataSourceBtn.IsEnabled = true;
                DeleteDataSourceBtn.IsEnabled = true;
            }
            else
            {
                EditDataSourceBtn.IsEnabled = false;
                DeleteDataSourceBtn.IsEnabled = false;
            }
        }

        private void DataSourceItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Handle item selection
        }

        private void AddDataSource_Click(object sender, RoutedEventArgs e)
        {
            _currentDataSource = new DataSource { Name = "New Data Source" };
            LoadDataSourceConfiguration(_currentDataSource);
            ConfigurationPanel.Visibility = Visibility.Visible;
            WelcomePanel.Visibility = Visibility.Collapsed;
        }

        private void EditDataSource_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDataSource != null)
            {
                LoadDataSourceConfiguration(_currentDataSource);
            }
        }

        private async void DeleteDataSource_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDataSource != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{_currentDataSource.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete from database if it has an ID (not a new item)
                        if (_currentDataSource.Id > 0)
                        {
                            await _dataSourceService.DeleteDataSourceAsync(_currentDataSource.Id);
                        }

                        DataSources.Remove(_currentDataSource);
                        ConfigurationPanel.Visibility = Visibility.Collapsed;
                        WelcomePanel.Visibility = Visibility.Visible;
                        _currentDataSource = null;

                        MessageBox.Show("Data source deleted successfully!", "Delete",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting data source: {ex.Message}", "Delete Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoadDataSourceConfiguration(DataSource dataSource)
        {
            // Load basic information
            NameTextBox.Text = dataSource.Name;
            DescriptionTextBox.Text = dataSource.Description;
            InterfaceComboBox.SelectedItem = dataSource.InterfaceType;
            ProtocolComboBox.SelectedItem = dataSource.ProtocolType;

            // Load configuration
            var config = dataSource.Configuration;

            // Common fields
            HostTextBox.Text = config.Host;
            PortTextBox.Text = config.Port.ToString();
            TimeoutTextBox.Text = config.Timeout.ToString();
            UseSSLCheckBox.IsChecked = config.UseSSL;
            UsernameTextBox.Text = config.Username;

            // Interface-specific fields
            FilePathTextBox.Text = config.FilePath;
            FilePatternTextBox.Text = config.FilePattern;
            WatchChangesCheckBox.IsChecked = config.WatchForChanges;

            // Serial fields
            SerialPortComboBox.SelectedItem = config.PortName;
            BaudRateComboBox.Text = config.BaudRate.ToString();
            DataBitsComboBox.Text = config.DataBits.ToString();
            ParityComboBox.Text = config.Parity;
            StopBitsComboBox.Text = config.StopBits;
            HandshakeComboBox.Text = config.Handshake;

            // Protocol fields
            TopicTextBox.Text = config.Topic;
            SlaveIdTextBox.Text = config.SlaveId.ToString();
            EndpointTextBox.Text = config.Endpoint;
            ApiKeyTextBox.Text = config.ApiKey;
            ClientIdTextBox.Text = config.ClientId;

            // Update UI visibility
            UpdateConfigurationVisibility();
        }

        private void Interface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConfigurationVisibility();
        }

        private void Protocol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConfigurationVisibility();
        }

        private void UpdateConfigurationVisibility()
        {
            // Hide all interface configs
            FileConfig.Visibility = Visibility.Collapsed;
            NetworkConfig.Visibility = Visibility.Collapsed;
            SerialConfig.Visibility = Visibility.Collapsed;
            USBConfig.Visibility = Visibility.Collapsed;

            // Hide all protocol configs
            ModbusConfig.Visibility = Visibility.Collapsed;
            MQTTConfig.Visibility = Visibility.Collapsed;
            APIConfig.Visibility = Visibility.Collapsed;

            // Show appropriate interface config
            if (InterfaceComboBox.SelectedItem is InterfaceType interfaceType)
            {
                InterfaceConfigGroup.Visibility = Visibility.Visible;

                switch (interfaceType)
                {
                    case InterfaceType.File:
                        FileConfig.Visibility = Visibility.Visible;
                        break;
                    case InterfaceType.TCP:
                    case InterfaceType.UDP:
                        NetworkConfig.Visibility = Visibility.Visible;
                        break;
                    case InterfaceType.Serial:
                        SerialConfig.Visibility = Visibility.Visible;
                        RefreshSerialPorts();
                        break;
                    case InterfaceType.USB:
                        USBConfig.Visibility = Visibility.Visible;
                        break;
                }
            }

            // Show appropriate protocol config
            if (ProtocolComboBox.SelectedItem is ProtocolType protocolType)
            {
                ProtocolConfigGroup.Visibility = Visibility.Visible;

                switch (protocolType)
                {
                    case ProtocolType.ModbusTCP:
                    case ProtocolType.ModbusRTU:
                        ModbusConfig.Visibility = Visibility.Visible;
                        break;
                    case ProtocolType.MQTT:
                        MQTTConfig.Visibility = Visibility.Visible;
                        break;
                    case ProtocolType.RestAPI:
                    case ProtocolType.SoapAPI:
                        APIConfig.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Data File",
                Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void RefreshUSBDevices_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement USB device enumeration
            USBDeviceComboBox.Items.Clear();
            USBDeviceComboBox.Items.Add("USB Device 1");
            USBDeviceComboBox.Items.Add("USB Device 2");
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDataSource == null) return;

            try
            {
                // Update current data source with UI values before testing
                UpdateDataSourceFromUI();

                // Show testing message
                TestConnectionBtn.IsEnabled = false;
                TestConnectionBtn.Content = "Testing...";

                // Test the connection using DataSourceService
                bool success = await _dataSourceService.TestConnectionAsync(_currentDataSource);

                if (success)
                {
                    MessageBox.Show("Connection test successful!", "Test Connection",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Connection test failed. Please check your settings.", "Test Connection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Test Connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TestConnectionBtn.IsEnabled = true;
                TestConnectionBtn.Content = "Test Connection";
            }
        }

        private async void SaveDataSource_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDataSource == null) return;

            try
            {
                // Update data source from UI
                UpdateDataSourceFromUI();

                // Save to database
                bool success = await _dataSourceService.SaveDataSourceAsync(_currentDataSource);

                if (success)
                {
                    // Add to collection if it's new (ID was 0)
                    if (!DataSources.Contains(_currentDataSource))
                    {
                        DataSources.Add(_currentDataSource);
                    }

                    MessageBox.Show("Data source saved successfully!", "Save",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to save data source to database.", "Save Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data source: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDataSourceFromUI()
        {
            if (_currentDataSource == null) return;

            // Update basic information
            _currentDataSource.Name = NameTextBox.Text;
            _currentDataSource.Description = DescriptionTextBox.Text;
            _currentDataSource.InterfaceType = (InterfaceType)InterfaceComboBox.SelectedItem;
            _currentDataSource.ProtocolType = (ProtocolType)ProtocolComboBox.SelectedItem;
            _currentDataSource.LastUpdated = DateTime.UtcNow;

            // Update configuration
            var config = _currentDataSource.Configuration;
            config.Host = HostTextBox.Text;
            if (int.TryParse(PortTextBox.Text, out int port)) config.Port = port;
            if (int.TryParse(TimeoutTextBox.Text, out int timeout)) config.Timeout = timeout;
            config.UseSSL = UseSSLCheckBox.IsChecked ?? false;
            config.Username = UsernameTextBox.Text;

            config.FilePath = FilePathTextBox.Text;
            config.FilePattern = FilePatternTextBox.Text;
            config.WatchForChanges = WatchChangesCheckBox.IsChecked ?? false;

            config.PortName = SerialPortComboBox.Text;
            if (int.TryParse(BaudRateComboBox.Text, out int baudRate)) config.BaudRate = baudRate;
            if (int.TryParse(DataBitsComboBox.Text, out int dataBits)) config.DataBits = dataBits;
            config.Parity = ParityComboBox.Text;
            config.StopBits = StopBitsComboBox.Text;
            config.Handshake = HandshakeComboBox.Text;

            config.Topic = TopicTextBox.Text;
            if (int.TryParse(SlaveIdTextBox.Text, out int slaveId)) config.SlaveId = slaveId;
            config.Endpoint = EndpointTextBox.Text;
            config.ApiKey = ApiKeyTextBox.Text;
            config.ClientId = ClientIdTextBox.Text;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationPanel.Visibility = Visibility.Collapsed;
            WelcomePanel.Visibility = Visibility.Visible;
            DataSourcesList.SelectedItem = null;
        }
    }
}
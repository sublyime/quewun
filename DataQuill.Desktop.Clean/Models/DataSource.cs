using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DataQuillDesktop.Models
{
    public enum InterfaceType
    {
        File,
        TCP,
        UDP,
        Serial,
        USB
    }

    // Modbus-specific enums and classes
    public enum ModbusDataFormat
    {
        UInt16,      // Unsigned 16-bit integer (1 register)
        SInt16,      // Signed 16-bit integer (1 register)
        UInt32,      // Unsigned 32-bit integer (2 registers)
        SInt32,      // Signed 32-bit integer (2 registers)
        Float32,     // 32-bit float (2 registers)
        Float32Swap  // 32-bit float with swapped registers (2 registers)
    }

    public class ModbusRegisterConfig
    {
        public string TagName { get; set; } = "";
        public int StartAddress { get; set; }
        public ModbusDataFormat DataFormat { get; set; } = ModbusDataFormat.UInt16;
        public string Description { get; set; } = "";
        public double Scale { get; set; } = 1.0;
        public double Offset { get; set; } = 0.0;
        public string Units { get; set; } = "";
    }

    public enum ProtocolType
    {
        ModbusTCP,
        ModbusRTU,
        MQTT,
        NMEA0183,
        HART,
        OPCUA,
        OSIPI,
        IP21,
        RestAPI,
        SoapAPI,
        IoTDevices,
        Other
    }

    public class DataSource : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private InterfaceType _interfaceType;
        private ProtocolType _protocolType;
        private string _connectionString = string.Empty;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime _lastUpdated;
        private DataSourceConfiguration _configuration = new();

        [Key]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [StringLength(500)]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public InterfaceType InterfaceType
        {
            get => _interfaceType;
            set => SetProperty(ref _interfaceType, value);
        }

        public ProtocolType ProtocolType
        {
            get => _protocolType;
            set => SetProperty(ref _protocolType, value);
        }

        [StringLength(1000)]
        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public DataSourceConfiguration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public DataSource()
        {
            _createdAt = DateTime.UtcNow;
            _lastUpdated = DateTime.UtcNow;
            _isActive = true;
        }

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

    public class DataSourceConfiguration : INotifyPropertyChanged
    {
        // Common properties
        private string _host = string.Empty;
        private int _port;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private int _timeout = 30;
        private int _retryAttempts = 3;
        private bool _useSSL;

        // File-specific properties
        private string _filePath = string.Empty;
        private string _filePattern = string.Empty;
        private bool _watchForChanges;

        // Serial-specific properties
        private string _portName = string.Empty;
        private int _baudRate = 9600;
        private int _dataBits = 8;
        private string _parity = "None";
        private string _stopBits = "One";
        private string _handshake = "None";

        // Protocol-specific properties
        private string _topic = string.Empty; // MQTT
        private int _slaveId = 1; // Modbus
        private string _endpoint = string.Empty; // OPC UA, API
        private string _apiKey = string.Empty;
        private string _clientId = string.Empty;
        private Dictionary<string, string> _customParameters = new();

        // Common Properties
        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        public int RetryAttempts
        {
            get => _retryAttempts;
            set => SetProperty(ref _retryAttempts, value);
        }

        public bool UseSSL
        {
            get => _useSSL;
            set => SetProperty(ref _useSSL, value);
        }

        // File Properties
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string FilePattern
        {
            get => _filePattern;
            set => SetProperty(ref _filePattern, value);
        }

        public bool WatchForChanges
        {
            get => _watchForChanges;
            set => SetProperty(ref _watchForChanges, value);
        }

        // Serial Properties
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        public string Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        public string StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        public string Handshake
        {
            get => _handshake;
            set => SetProperty(ref _handshake, value);
        }

        // Protocol Properties
        public string Topic
        {
            get => _topic;
            set => SetProperty(ref _topic, value);
        }

        public int SlaveId
        {
            get => _slaveId;
            set => SetProperty(ref _slaveId, value);
        }

        public string Endpoint
        {
            get => _endpoint;
            set => SetProperty(ref _endpoint, value);
        }

        public string ApiKey
        {
            get => _apiKey;
            set => SetProperty(ref _apiKey, value);
        }

        public string ClientId
        {
            get => _clientId;
            set => SetProperty(ref _clientId, value);
        }

        public Dictionary<string, string> CustomParameters
        {
            get => _customParameters;
            set => SetProperty(ref _customParameters, value);
        }

        // Modbus-specific configuration
        private List<ModbusRegisterConfig> _modbusRegisters = new();
        private ModbusDataFormat _defaultDataFormat = ModbusDataFormat.UInt16;

        public List<ModbusRegisterConfig> ModbusRegisters
        {
            get => _modbusRegisters;
            set => SetProperty(ref _modbusRegisters, value);
        }

        public ModbusDataFormat DefaultDataFormat
        {
            get => _defaultDataFormat;
            set => SetProperty(ref _defaultDataFormat, value);
        }

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
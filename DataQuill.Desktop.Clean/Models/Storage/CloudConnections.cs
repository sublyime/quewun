using System;

namespace DataQuillDesktop.Models.Storage
{
    /// <summary>
    /// AWS connection configuration
    /// </summary>
    public class AwsConnection : CloudConnectionBase
    {
        private string _accessKeyId = string.Empty;
        private string _secretAccessKey = string.Empty;
        private string _region = "us-east-1";
        private string _sessionToken = string.Empty;
        private bool _useSessionToken = false;

        public AwsConnection()
        {
            Provider = CloudProvider.AWS;
        }

        public string AccessKeyId
        {
            get => _accessKeyId;
            set => SetProperty(ref _accessKeyId, value);
        }

        public string SecretAccessKey
        {
            get => _secretAccessKey;
            set => SetProperty(ref _secretAccessKey, value);
        }

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string SessionToken
        {
            get => _sessionToken;
            set => SetProperty(ref _sessionToken, value);
        }

        public bool UseSessionToken
        {
            get => _useSessionToken;
            set => SetProperty(ref _useSessionToken, value);
        }

        public override AuthenticationType AuthType => AuthenticationType.AccessKeySecret;

        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(AccessKeyId) || string.IsNullOrWhiteSpace(SecretAccessKey))
                return false;

            if (UseSessionToken && string.IsNullOrWhiteSpace(SessionToken))
                return false;

            return !string.IsNullOrWhiteSpace(Region);
        }

        public override string GetConnectionIdentifier()
        {
            return $"aws://{AccessKeyId}@{Region}";
        }
    }

    /// <summary>
    /// Google Cloud Platform connection configuration
    /// </summary>
    public class GoogleCloudConnection : CloudConnectionBase
    {
        private string _projectId = string.Empty;
        private string _serviceAccountJson = string.Empty;
        private string _keyFilePath = string.Empty;
        private bool _useKeyFile = false;

        public GoogleCloudConnection()
        {
            Provider = CloudProvider.GoogleCloud;
        }

        public string ProjectId
        {
            get => _projectId;
            set => SetProperty(ref _projectId, value);
        }

        public string ServiceAccountJson
        {
            get => _serviceAccountJson;
            set => SetProperty(ref _serviceAccountJson, value);
        }

        public string KeyFilePath
        {
            get => _keyFilePath;
            set => SetProperty(ref _keyFilePath, value);
        }

        public bool UseKeyFile
        {
            get => _useKeyFile;
            set => SetProperty(ref _useKeyFile, value);
        }

        public override AuthenticationType AuthType => AuthenticationType.ServiceAccountJson;

        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
                return false;

            if (UseKeyFile)
                return !string.IsNullOrWhiteSpace(KeyFilePath) && System.IO.File.Exists(KeyFilePath);

            return !string.IsNullOrWhiteSpace(ServiceAccountJson);
        }

        public override string GetConnectionIdentifier()
        {
            return $"gcp://{ProjectId}";
        }
    }

    /// <summary>
    /// Microsoft Azure connection configuration
    /// </summary>
    public class AzureConnection : CloudConnectionBase
    {
        private string _connectionString = string.Empty;
        private string _tenantId = string.Empty;
        private string _clientId = string.Empty;
        private string _clientSecret = string.Empty;
        private string _subscriptionId = string.Empty;
        private bool _useManagedIdentity = false;

        public AzureConnection()
        {
            Provider = CloudProvider.Azure;
        }

        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        public string TenantId
        {
            get => _tenantId;
            set => SetProperty(ref _tenantId, value);
        }

        public string ClientId
        {
            get => _clientId;
            set => SetProperty(ref _clientId, value);
        }

        public string ClientSecret
        {
            get => _clientSecret;
            set => SetProperty(ref _clientSecret, value);
        }

        public string SubscriptionId
        {
            get => _subscriptionId;
            set => SetProperty(ref _subscriptionId, value);
        }

        public bool UseManagedIdentity
        {
            get => _useManagedIdentity;
            set => SetProperty(ref _useManagedIdentity, value);
        }

        public override AuthenticationType AuthType =>
            UseManagedIdentity ? AuthenticationType.ManagedIdentity :
            !string.IsNullOrEmpty(ConnectionString) ? AuthenticationType.ConnectionString :
            AuthenticationType.OAuth2;

        public override bool IsValid()
        {
            if (UseManagedIdentity)
                return !string.IsNullOrWhiteSpace(SubscriptionId);

            if (!string.IsNullOrWhiteSpace(ConnectionString))
                return true;

            return !string.IsNullOrWhiteSpace(TenantId) &&
                   !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(ClientSecret) &&
                   !string.IsNullOrWhiteSpace(SubscriptionId);
        }

        public override string GetConnectionIdentifier()
        {
            if (UseManagedIdentity)
                return $"azure://managed-identity@{SubscriptionId}";

            if (!string.IsNullOrEmpty(ConnectionString))
                return "azure://connection-string";

            return $"azure://{ClientId}@{TenantId}";
        }
    }

    /// <summary>
    /// Oracle Cloud Infrastructure connection configuration
    /// </summary>
    public class OracleConnection : CloudConnectionBase
    {
        private string _userId = string.Empty;
        private string _fingerprint = string.Empty;
        private string _tenancyId = string.Empty;
        private string _region = string.Empty;
        private string _privateKeyPath = string.Empty;
        private string _privateKeyContent = string.Empty;
        private bool _useKeyFile = true;

        public OracleConnection()
        {
            Provider = CloudProvider.Oracle;
        }

        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public string Fingerprint
        {
            get => _fingerprint;
            set => SetProperty(ref _fingerprint, value);
        }

        public string TenancyId
        {
            get => _tenancyId;
            set => SetProperty(ref _tenancyId, value);
        }

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string PrivateKeyPath
        {
            get => _privateKeyPath;
            set => SetProperty(ref _privateKeyPath, value);
        }

        public string PrivateKeyContent
        {
            get => _privateKeyContent;
            set => SetProperty(ref _privateKeyContent, value);
        }

        public bool UseKeyFile
        {
            get => _useKeyFile;
            set => SetProperty(ref _useKeyFile, value);
        }

        public override AuthenticationType AuthType => AuthenticationType.AccessKeySecret;

        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(UserId) ||
                string.IsNullOrWhiteSpace(Fingerprint) ||
                string.IsNullOrWhiteSpace(TenancyId) ||
                string.IsNullOrWhiteSpace(Region))
                return false;

            if (UseKeyFile)
                return !string.IsNullOrWhiteSpace(PrivateKeyPath) && System.IO.File.Exists(PrivateKeyPath);

            return !string.IsNullOrWhiteSpace(PrivateKeyContent);
        }

        public override string GetConnectionIdentifier()
        {
            return $"oracle://{UserId}@{Region}";
        }
    }
}
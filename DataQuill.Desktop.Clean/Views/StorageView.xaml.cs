using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataQuillDesktop.Models.Storage;
using DataQuillDesktop.Services;

namespace DataQuillDesktop.Views
{
    public partial class StorageView : UserControl
    {
        private CloudConnectionManager _connectionManager;
        private CloudConnectionBase? _currentEditingConnection;
        private bool _isEditing = false;

        public StorageView()
        {
            InitializeComponent();
            _connectionManager = CloudConnectionManager.Instance;
            LoadConnections();
        }

        private void LoadConnections()
        {
            ConnectionsContainer.Children.Clear();

            if (_connectionManager.Connections.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;

            foreach (var connection in _connectionManager.Connections)
            {
                var connectionPanel = CreateConnectionPanel(connection);
                ConnectionsContainer.Children.Add(connectionPanel);
            }
        }

        private Border CreateConnectionPanel(CloudConnectionBase connection)
        {
            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(16, 12, 16, 12),
                Background = System.Windows.Media.Brushes.Transparent
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

            // Provider icon
            var providerIcon = new TextBlock
            {
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            switch (connection.Provider)
            {
                case CloudProvider.AWS:
                    providerIcon.Text = "☁️";
                    providerIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    break;
                case CloudProvider.GoogleCloud:
                    providerIcon.Text = "🌐";
                    providerIcon.Foreground = System.Windows.Media.Brushes.Blue;
                    break;
                case CloudProvider.Azure:
                    providerIcon.Text = "⚡";
                    providerIcon.Foreground = System.Windows.Media.Brushes.CornflowerBlue;
                    break;
                case CloudProvider.Oracle:
                    providerIcon.Text = "🏛️";
                    providerIcon.Foreground = System.Windows.Media.Brushes.Red;
                    break;
            }

            Grid.SetColumn(providerIcon, 0);
            grid.Children.Add(providerIcon);

            // Connection info
            var infoPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };

            var nameText = new TextBlock
            {
                Text = connection.Name,
                FontWeight = FontWeights.Medium,
                FontSize = 14
            };
            infoPanel.Children.Add(nameText);

            var descText = new TextBlock
            {
                Text = connection.Description,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray
            };
            infoPanel.Children.Add(descText);

            var providerText = new TextBlock
            {
                Text = connection.Provider.ToString(),
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.LightGray
            };
            infoPanel.Children.Add(providerText);

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Status
            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var statusIndicator = new System.Windows.Shapes.Ellipse
            {
                Width = 12,
                Height = 12,
                Margin = new Thickness(0, 0, 8, 0)
            };

            switch (connection.Status)
            {
                case ConnectionStatus.Connected:
                    statusIndicator.Fill = System.Windows.Media.Brushes.Green;
                    break;
                case ConnectionStatus.Disconnected:
                    statusIndicator.Fill = System.Windows.Media.Brushes.Gray;
                    break;
                case ConnectionStatus.Connecting:
                    statusIndicator.Fill = System.Windows.Media.Brushes.Orange;
                    break;
                case ConnectionStatus.Failed:
                    statusIndicator.Fill = System.Windows.Media.Brushes.Red;
                    break;
                case ConnectionStatus.Testing:
                    statusIndicator.Fill = System.Windows.Media.Brushes.Yellow;
                    break;
            }

            statusPanel.Children.Add(statusIndicator);

            var statusText = new TextBlock
            {
                Text = connection.Status.ToString(),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            statusPanel.Children.Add(statusText);

            Grid.SetColumn(statusPanel, 2);
            grid.Children.Add(statusPanel);

            // Action buttons
            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var testButton = new Button
            {
                Content = "Test",
                Style = (Style)FindResource("SecondaryButtonStyle"),
                FontSize = 11,
                Padding = new Thickness(8, 4, 8, 4),
                Tag = connection
            };
            testButton.Click += TestConnection_Click;
            actionPanel.Children.Add(testButton);

            var connectButton = new Button
            {
                Content = connection.Status == ConnectionStatus.Connected ? "Disconnect" : "Connect",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                FontSize = 11,
                Padding = new Thickness(8, 4, 8, 4),
                Tag = connection
            };

            if (connection.Status == ConnectionStatus.Connected)
            {
                connectButton.Background = System.Windows.Media.Brushes.Red;
            }

            connectButton.Click += ConnectDisconnect_Click;
            actionPanel.Children.Add(connectButton);

            var editButton = new Button
            {
                Content = "Edit",
                Style = (Style)FindResource("SecondaryButtonStyle"),
                FontSize = 11,
                Padding = new Thickness(8, 4, 8, 4),
                Tag = connection
            };
            editButton.Click += EditConnection_Click;
            actionPanel.Children.Add(editButton);

            var deleteButton = new Button
            {
                Content = "Delete",
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 11,
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(4, 4, 4, 4),
                Tag = connection
            };
            deleteButton.Click += DeleteConnection_Click;
            actionPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionPanel, 3);
            grid.Children.Add(actionPanel);

            border.Child = grid;
            return border;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadConnections();
        }

        private void AddConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEditPanel(true);
        }

        private void ShowEditPanel(bool isAddMode, CloudConnectionBase? connection = null)
        {
            _isEditing = !isAddMode;
            _currentEditingConnection = connection;

            PanelHeader.Text = isAddMode ? "Add New Connection" : "Edit Connection";
            ProviderSelectionPanel.Visibility = isAddMode ? Visibility.Visible : Visibility.Collapsed;

            if (isAddMode)
            {
                ProviderComboBox.SelectedIndex = 0;
                CreateConnectionForm(CloudProvider.AWS);
            }
            else if (connection != null)
            {
                CreateConnectionForm(connection.Provider, connection);
            }

            EditPanel.Visibility = Visibility.Visible;
        }

        private void CreateConnectionForm(CloudProvider provider, CloudConnectionBase? existingConnection = null)
        {
            ConnectionFormContainer.Children.Clear();

            // Connection Name
            var nameLabel = new TextBlock { Text = "Connection Name", Margin = new Thickness(0, 0, 0, 4) };
            var nameTextBox = new TextBox
            {
                Name = "ConnectionName",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existingConnection?.Name ?? $"New {provider} Connection"
            };
            ConnectionFormContainer.Children.Add(nameLabel);
            ConnectionFormContainer.Children.Add(nameTextBox);

            // Description
            var descLabel = new TextBlock { Text = "Description (Optional)", Margin = new Thickness(0, 0, 0, 4) };
            var descTextBox = new TextBox
            {
                Name = "ConnectionDescription",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existingConnection?.Description ?? ""
            };
            ConnectionFormContainer.Children.Add(descLabel);
            ConnectionFormContainer.Children.Add(descTextBox);

            // Provider-specific fields
            switch (provider)
            {
                case CloudProvider.AWS:
                    CreateAwsForm(existingConnection as AwsConnection);
                    break;
                case CloudProvider.GoogleCloud:
                    CreateGoogleCloudForm(existingConnection as GoogleCloudConnection);
                    break;
                case CloudProvider.Azure:
                    CreateAzureForm(existingConnection as AzureConnection);
                    break;
                case CloudProvider.Oracle:
                    CreateOracleForm(existingConnection as OracleConnection);
                    break;
            }
        }

        private void CreateAwsForm(AwsConnection? existing = null)
        {
            // Access Key ID
            var accessKeyLabel = new TextBlock { Text = "Access Key ID", Margin = new Thickness(0, 0, 0, 4) };
            var accessKeyTextBox = new TextBox
            {
                Name = "AccessKeyId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.AccessKeyId ?? ""
            };
            ConnectionFormContainer.Children.Add(accessKeyLabel);
            ConnectionFormContainer.Children.Add(accessKeyTextBox);

            // Secret Access Key
            var secretKeyLabel = new TextBlock { Text = "Secret Access Key", Margin = new Thickness(0, 0, 0, 4) };
            var secretKeyTextBox = new PasswordBox
            {
                Name = "SecretAccessKey",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12)
            };
            ConnectionFormContainer.Children.Add(secretKeyLabel);
            ConnectionFormContainer.Children.Add(secretKeyTextBox);

            // Region
            var regionLabel = new TextBlock { Text = "Region", Margin = new Thickness(0, 0, 0, 4) };
            var regionComboBox = new ComboBox
            {
                Name = "Region",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var regions = new[] { "us-east-1", "us-west-1", "us-west-2", "eu-west-1", "eu-central-1", "ap-southeast-1", "ap-northeast-1" };
            foreach (var region in regions)
            {
                regionComboBox.Items.Add(new ComboBoxItem { Content = region });
            }

            regionComboBox.SelectedIndex = 0;
            if (existing != null)
            {
                for (int i = 0; i < regions.Length; i++)
                {
                    if (regions[i] == existing.Region)
                    {
                        regionComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            ConnectionFormContainer.Children.Add(regionLabel);
            ConnectionFormContainer.Children.Add(regionComboBox);
        }

        private void CreateGoogleCloudForm(GoogleCloudConnection? existing = null)
        {
            // Project ID
            var projectIdLabel = new TextBlock { Text = "Project ID", Margin = new Thickness(0, 0, 0, 4) };
            var projectIdTextBox = new TextBox
            {
                Name = "ProjectId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.ProjectId ?? ""
            };
            ConnectionFormContainer.Children.Add(projectIdLabel);
            ConnectionFormContainer.Children.Add(projectIdTextBox);

            // Service Account JSON
            var serviceAccountLabel = new TextBlock { Text = "Service Account JSON", Margin = new Thickness(0, 0, 0, 4) };
            var serviceAccountTextBox = new TextBox
            {
                Name = "ServiceAccountJson",
                AcceptsReturn = true,
                Height = 120,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.ServiceAccountJson ?? ""
            };
            ConnectionFormContainer.Children.Add(serviceAccountLabel);
            ConnectionFormContainer.Children.Add(serviceAccountTextBox);
        }

        private void CreateAzureForm(AzureConnection? existing = null)
        {
            // Subscription ID
            var subscriptionIdLabel = new TextBlock { Text = "Subscription ID", Margin = new Thickness(0, 0, 0, 4) };
            var subscriptionIdTextBox = new TextBox
            {
                Name = "SubscriptionId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.SubscriptionId ?? ""
            };
            ConnectionFormContainer.Children.Add(subscriptionIdLabel);
            ConnectionFormContainer.Children.Add(subscriptionIdTextBox);

            // Tenant ID
            var tenantIdLabel = new TextBlock { Text = "Tenant ID", Margin = new Thickness(0, 0, 0, 4) };
            var tenantIdTextBox = new TextBox
            {
                Name = "TenantId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.TenantId ?? ""
            };
            ConnectionFormContainer.Children.Add(tenantIdLabel);
            ConnectionFormContainer.Children.Add(tenantIdTextBox);

            // Client ID
            var clientIdLabel = new TextBlock { Text = "Client ID", Margin = new Thickness(0, 0, 0, 4) };
            var clientIdTextBox = new TextBox
            {
                Name = "ClientId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.ClientId ?? ""
            };
            ConnectionFormContainer.Children.Add(clientIdLabel);
            ConnectionFormContainer.Children.Add(clientIdTextBox);

            // Client Secret
            var clientSecretLabel = new TextBlock { Text = "Client Secret", Margin = new Thickness(0, 0, 0, 4) };
            var clientSecretTextBox = new PasswordBox
            {
                Name = "ClientSecret",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12)
            };
            ConnectionFormContainer.Children.Add(clientSecretLabel);
            ConnectionFormContainer.Children.Add(clientSecretTextBox);
        }

        private void CreateOracleForm(OracleConnection? existing = null)
        {
            // User ID
            var userIdLabel = new TextBlock { Text = "User ID", Margin = new Thickness(0, 0, 0, 4) };
            var userIdTextBox = new TextBox
            {
                Name = "UserId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.UserId ?? ""
            };
            ConnectionFormContainer.Children.Add(userIdLabel);
            ConnectionFormContainer.Children.Add(userIdTextBox);

            // Tenancy ID
            var tenancyIdLabel = new TextBlock { Text = "Tenancy ID", Margin = new Thickness(0, 0, 0, 4) };
            var tenancyIdTextBox = new TextBox
            {
                Name = "TenancyId",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.TenancyId ?? ""
            };
            ConnectionFormContainer.Children.Add(tenancyIdLabel);
            ConnectionFormContainer.Children.Add(tenancyIdTextBox);

            // Fingerprint
            var fingerprintLabel = new TextBlock { Text = "Fingerprint", Margin = new Thickness(0, 0, 0, 4) };
            var fingerprintTextBox = new TextBox
            {
                Name = "Fingerprint",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.Fingerprint ?? ""
            };
            ConnectionFormContainer.Children.Add(fingerprintLabel);
            ConnectionFormContainer.Children.Add(fingerprintTextBox);

            // Region
            var regionLabel = new TextBlock { Text = "Region", Margin = new Thickness(0, 0, 0, 4) };
            var regionComboBox = new ComboBox
            {
                Name = "Region",
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var regions = new[] { "us-ashburn-1", "us-phoenix-1", "eu-frankfurt-1", "ap-tokyo-1" };
            foreach (var region in regions)
            {
                regionComboBox.Items.Add(new ComboBoxItem { Content = region });
            }

            regionComboBox.SelectedIndex = 0;
            ConnectionFormContainer.Children.Add(regionLabel);
            ConnectionFormContainer.Children.Add(regionComboBox);

            // Private Key Content
            var privateKeyLabel = new TextBlock { Text = "Private Key Content", Margin = new Thickness(0, 0, 0, 4) };
            var privateKeyTextBox = new TextBox
            {
                Name = "PrivateKeyContent",
                AcceptsReturn = true,
                Height = 120,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 12),
                Text = existing?.PrivateKeyContent ?? ""
            };
            ConnectionFormContainer.Children.Add(privateKeyLabel);
            ConnectionFormContainer.Children.Add(privateKeyTextBox);
        }

        private void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProviderComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string providerTag)
            {
                if (Enum.TryParse<CloudProvider>(providerTag, out var provider))
                {
                    CreateConnectionForm(provider);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EditPanel.Visibility = Visibility.Collapsed;
            _currentEditingConnection = null;
            _isEditing = false;
        }

        private void SaveConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connection = CreateConnectionFromForm();
                if (connection == null) return;

                if (_isEditing && _currentEditingConnection != null)
                {
                    connection.Id = _currentEditingConnection.Id;
                    _connectionManager.UpdateConnection(connection);
                }
                else
                {
                    _connectionManager.AddConnection(connection);
                }

                LoadConnections();
                EditPanel.Visibility = Visibility.Collapsed;
                _currentEditingConnection = null;
                _isEditing = false;

                MessageBox.Show("Connection saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private CloudConnectionBase? CreateConnectionFromForm()
        {
            var nameTextBox = FindChild<TextBox>(ConnectionFormContainer, "ConnectionName");
            var descTextBox = FindChild<TextBox>(ConnectionFormContainer, "ConnectionDescription");

            if (nameTextBox == null || string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a connection name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            CloudProvider provider;
            if (ProviderSelectionPanel.Visibility == Visibility.Visible)
            {
                var selectedItem = ProviderComboBox.SelectedItem as ComboBoxItem;
                if (!Enum.TryParse<CloudProvider>(selectedItem?.Tag?.ToString(), out provider))
                {
                    MessageBox.Show("Please select a cloud provider.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            else
            {
                provider = _currentEditingConnection?.Provider ?? CloudProvider.AWS;
            }

            CloudConnectionBase connection = provider switch
            {
                CloudProvider.AWS => CreateAwsConnectionFromForm(),
                CloudProvider.GoogleCloud => CreateGoogleCloudConnectionFromForm(),
                CloudProvider.Azure => CreateAzureConnectionFromForm(),
                CloudProvider.Oracle => CreateOracleConnectionFromForm(),
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };

            connection.Name = nameTextBox.Text;
            connection.Description = descTextBox?.Text ?? "";

            return connection;
        }

        private AwsConnection CreateAwsConnectionFromForm()
        {
            var accessKeyTextBox = FindChild<TextBox>(ConnectionFormContainer, "AccessKeyId");
            var secretKeyBox = FindChild<PasswordBox>(ConnectionFormContainer, "SecretAccessKey");
            var regionComboBox = FindChild<ComboBox>(ConnectionFormContainer, "Region");

            var connection = new AwsConnection
            {
                AccessKeyId = accessKeyTextBox?.Text ?? "",
                SecretAccessKey = secretKeyBox?.Password ?? "",
                Region = (regionComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "us-east-1"
            };

            return connection;
        }

        private GoogleCloudConnection CreateGoogleCloudConnectionFromForm()
        {
            var projectIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "ProjectId");
            var serviceAccountTextBox = FindChild<TextBox>(ConnectionFormContainer, "ServiceAccountJson");

            var connection = new GoogleCloudConnection
            {
                ProjectId = projectIdTextBox?.Text ?? "",
                ServiceAccountJson = serviceAccountTextBox?.Text ?? ""
            };

            return connection;
        }

        private AzureConnection CreateAzureConnectionFromForm()
        {
            var subscriptionIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "SubscriptionId");
            var tenantIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "TenantId");
            var clientIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "ClientId");
            var clientSecretBox = FindChild<PasswordBox>(ConnectionFormContainer, "ClientSecret");

            var connection = new AzureConnection
            {
                SubscriptionId = subscriptionIdTextBox?.Text ?? "",
                TenantId = tenantIdTextBox?.Text ?? "",
                ClientId = clientIdTextBox?.Text ?? "",
                ClientSecret = clientSecretBox?.Password ?? ""
            };

            return connection;
        }

        private OracleConnection CreateOracleConnectionFromForm()
        {
            var userIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "UserId");
            var tenancyIdTextBox = FindChild<TextBox>(ConnectionFormContainer, "TenancyId");
            var fingerprintTextBox = FindChild<TextBox>(ConnectionFormContainer, "Fingerprint");
            var regionComboBox = FindChild<ComboBox>(ConnectionFormContainer, "Region");
            var privateKeyTextBox = FindChild<TextBox>(ConnectionFormContainer, "PrivateKeyContent");

            var connection = new OracleConnection
            {
                UserId = userIdTextBox?.Text ?? "",
                TenancyId = tenancyIdTextBox?.Text ?? "",
                Fingerprint = fingerprintTextBox?.Text ?? "",
                Region = (regionComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "us-ashburn-1",
                PrivateKeyContent = privateKeyTextBox?.Text ?? "",
                UseKeyFile = false
            };

            return connection;
        }

        private T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            var foundChild = default(T);
            var childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && (child as FrameworkElement)?.Name == childName)
                {
                    foundChild = typedChild;
                    break;
                }

                foundChild = FindChild<T>(child, childName);
                if (foundChild != null) break;
            }

            return foundChild;
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CloudConnectionBase connection)
            {
                button.IsEnabled = false;
                button.Content = "Testing...";

                try
                {
                    var (success, errorMessage) = await _connectionManager.TestConnectionAsync(connection);

                    if (success)
                    {
                        MessageBox.Show("Connection test successful!", "Test Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Connection test failed: {errorMessage}", "Test Result", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error testing connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    button.IsEnabled = true;
                    button.Content = "Test";
                    LoadConnections(); // Refresh to show updated status
                }
            }
        }

        private async void ConnectDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CloudConnectionBase connection)
            {
                button.IsEnabled = false;

                try
                {
                    if (connection.Status == ConnectionStatus.Connected)
                    {
                        _connectionManager.Disconnect(connection.Id);
                        MessageBox.Show("Disconnected successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        button.Content = "Connecting...";
                        var success = await _connectionManager.ConnectAsync(connection.Id);

                        if (success)
                        {
                            MessageBox.Show("Connected successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Connection failed. Please check your credentials.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    button.IsEnabled = true;
                    LoadConnections(); // Refresh to show updated status
                }
            }
        }

        private void EditConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CloudConnectionBase connection)
            {
                ShowEditPanel(false, connection);
            }
        }

        private void DeleteConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CloudConnectionBase connection)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the connection '{connection.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _connectionManager.RemoveConnection(connection.Id);
                    LoadConnections();
                    MessageBox.Show("Connection deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using DataQuillDesktop.Models;

namespace DataQuillDesktop.Views
{
    public partial class UserEditDialog : Window
    {
        public User? User { get; private set; }
        private readonly bool isEdit;

        public UserEditDialog(User? user = null)
        {
            InitializeComponent();
            if (user != null)
            {
                isEdit = true;
                User = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    Role = user.Role
                };
                UsernameBox.Text = User.Username;
                EmailBox.Text = User.Email;
                IsAdminBox.IsChecked = User.IsAdmin;
                RoleBox.SelectedItem = user.Role == "Admin" ? RoleBox.Items[1] : RoleBox.Items[0];
            }
            else
            {
                isEdit = false;
                User = new User();
                RoleBox.SelectedIndex = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            User!.Username = UsernameBox.Text.Trim();
            User.Email = EmailBox.Text.Trim();
            User.IsAdmin = IsAdminBox.IsChecked == true;
            User.Role = (RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "User";
            if (!isEdit && !string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                User.PasswordHash = PasswordBox.Password; // TODO: Hash in real app
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
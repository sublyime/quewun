using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataQuillDesktop.Models;

namespace DataQuillDesktop.Views
{
    public partial class UserAdminView : UserControl
    {
        public ObservableCollection<User> Users { get; set; } = new();

        public UserAdminView()
        {
            InitializeComponent();
            LoadUsers();
            UsersDataGrid.ItemsSource = Users;
            AddUserButton.Click += AddUserButton_Click;
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private ObservableCollection<User> AllUsers { get; set; } = new();

        private void LoadUsers()
        {
            using var db = new QuillDbContext();
            AllUsers.Clear();
            foreach (var user in db.Users.ToList())
                AllUsers.Add(user);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filter = SearchBox.Text.Trim().ToLower();
            Users.Clear();
            foreach (var user in AllUsers)
            {
                if (string.IsNullOrEmpty(filter) ||
                    user.Username.ToLower().Contains(filter) ||
                    user.Email.ToLower().Contains(filter) ||
                    user.Role.ToLower().Contains(filter))
                {
                    Users.Add(user);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.User != null)
            {
                using var db = new QuillDbContext();
                db.Users.Add(dialog.User);
                db.SaveChanges();
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is User user)
            {
                var dialog = new UserEditDialog(user);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true && dialog.User != null)
                {
                    using var db = new QuillDbContext();
                    var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        dbUser.Username = dialog.User.Username;
                        dbUser.Email = dialog.User.Email;
                        dbUser.IsAdmin = dialog.User.IsAdmin;
                        // Optionally update password if changed
                        if (!string.IsNullOrWhiteSpace(dialog.User.PasswordHash))
                            dbUser.PasswordHash = dialog.User.PasswordHash;
                        dbUser.Role = dialog.User.Role;
                        db.SaveChanges();
                        LoadUsers();
                    }
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is User user)
            {
                if (MessageBox.Show($"Delete user '{user.Username}'?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using var db = new QuillDbContext();
                    var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        db.Users.Remove(dbUser);
                        db.SaveChanges();
                        LoadUsers();
                    }
                }
            }
        }
    }
}
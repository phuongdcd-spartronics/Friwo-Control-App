using FriwoControl.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for UserManagementPage.xaml
    /// </summary>
    public partial class UserManagementPage : Page
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();

        public ObservableCollection<User> UserList { get; private set; }
        public ObservableCollection<CheckBoxModel> RoleList { get; private set; }
        public ObservableCollection<History> HistoryList { get; private set; }

        public class CheckBoxModel
        {
            public string Name { get; set; }
            public bool Checked { get; set; }
        }

        public UserManagementPage()
        {
            InitializeComponent();

            UserList = new ObservableCollection<User>(
                _dbContext.Users.ToList());
            RoleList = new ObservableCollection<CheckBoxModel>(
                _dbContext.Roles.Select(r => new CheckBoxModel() { Name = r.Id, Checked = false })
            );
            HistoryList = new ObservableCollection<History>();

            this.DataContext = this;
        }

        private void gvUsers_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var user = gvUsers.SelectedItem as User;
            if (user != null)
            {
                // Get all roles of selected user
                RoleList.Clear();
                foreach (Role role in _dbContext.Roles.ToList())
                {
                    RoleList.Add(new CheckBoxModel() { Name = role.Id, Checked = user.Roles.Contains(role.Id) });
                }

                // Get 100 latest history recorded of selected user
                HistoryList.Clear();
                foreach (History history in _dbContext.Histories.Where(h => h.CreatedBy == user.Username)
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(100))
                {
                    HistoryList.Add(history);
                }
            }
        }

        private void btnSaveRole_Click(object sender, RoutedEventArgs e)
        {
            var user = gvUsers.SelectedItem as User;
            if (user == null)
            {
                CstMessageBox.Show("User not found", "Please select an user and try again!", CstMessageBoxIcon.Warning);
                return;
            }

            string roles = "";
            foreach (var chk in RoleList.Where(r => r.Checked))
            {
                roles += chk.Name + ";";
            }
            user.Roles = roles;
            _dbContext.SaveChanges();
            CstMessageBox.Show("Success", $"Saved roles for user {user.Username}", CstMessageBoxIcon.Success);
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Password;
            string roles = "";

            // Validate username
            if (string.IsNullOrEmpty(username))
            {
                CstMessageBox.Show("Username is required", "Please enter username!", CstMessageBoxIcon.Warning);
                return;
            }
            // Validate full name
            else if (string.IsNullOrEmpty(fullName))
            {
                CstMessageBox.Show("Full name is required", "Please enter full name!", CstMessageBoxIcon.Warning);
                return;
            }
            // Validate password
            else if (string.IsNullOrEmpty(password))
            {
                CstMessageBox.Show("Password is required", "Please enter password!", CstMessageBoxIcon.Warning);
                return;
            }
            else if (UserList.Any(u => u.Username == username))
            {
                CstMessageBox.Show("Duplicated", $"{username} is already existed!", CstMessageBoxIcon.Error);
                return;
            }

            // Get all selected roles
            foreach (var chk in RoleList.Where(r => r.Checked))
            {
                roles += chk.Name + ";";
            }
            // Validate role
            if (string.IsNullOrEmpty(roles))
            {
                CstMessageBox.Show("Roles is required", "Please select at least 1 role for user!", CstMessageBoxIcon.Warning);
                return;
            }

            var newUser = new User()
            {
                Username = username,
                Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)),
                FullName = fullName,
                Roles = roles
            };
            _dbContext.Users.Add(newUser);

            // Add user to grid if saved successful
            if (_dbContext.SaveChanges() > 0)
            {
                UserList.Add(newUser);
            }
        }

        private void gvUsers_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var user = gvUsers.SelectedItem as User;
            if (e.Key == Key.Delete && user != null)
            {
                // Prevent delete admin user
                if (user.Roles.Contains(DBEnum.Roles.ADMIN))
                {
                    CstMessageBox.Show("Forbidden", "You don't have permission to delete this user!", CstMessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show($"Are you sure to delete {user.Username} ({user.FullName})?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _dbContext.Users.Remove(user);
                    if (_dbContext.SaveChanges() > 0)
                    {
                        UserList.Remove(user);
                    }
                }
            }
        }
    }
}

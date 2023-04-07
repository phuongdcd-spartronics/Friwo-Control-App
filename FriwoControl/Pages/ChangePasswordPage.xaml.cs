using FriwoControl.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();

        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            User user = _dbContext.Users.FirstOrDefault(u => u.Username == UserSession.User.Username);
            string oldPassword = txtCurrentPassword.Password;
            string newPassword = txtNewPassword.Password;

            if (user == null)
            {
                CstMessageBox.Show("User not found", "Cannot find your user. Please log out and login again!", CstMessageBoxIcon.Warning);
                return;
            }
            else if (String.IsNullOrEmpty(oldPassword))
            {
                CstMessageBox.Show("Current password is required", "Please enter your current password!", CstMessageBoxIcon.Warning);
                return;
            }
            else if (String.IsNullOrEmpty(newPassword))
            {
                CstMessageBox.Show("New password is required", "Please enter your new password!", CstMessageBoxIcon.Warning);
                return;
            }
            else if (user.Password != Convert.ToBase64String(Encoding.UTF8.GetBytes(oldPassword)))
            {
                CstMessageBox.Show("Incorrect password", "Please make sure your current password is correct!", CstMessageBoxIcon.Error);
                return;
            }

            user.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(newPassword));
            _dbContext.SaveChanges();
            CstMessageBox.Show("Success", "Your password has been changed!", CstMessageBoxIcon.Success);
            txtCurrentPassword.Password = null;
            txtNewPassword.Password = null;
        }
    }
}

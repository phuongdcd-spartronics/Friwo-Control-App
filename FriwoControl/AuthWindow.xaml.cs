using FriwoControl.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private RadCallout _calloutCapslock = null;

        public AuthWindow()
        {
            InitializeComponent();

            _calloutCapslock = new RadCallout()
            {
                Content = "Capslock is on",
                UseLayoutRounding = true,

                ArrowType = CalloutArrowType.Triangle,
                ArrowAnchorPoint = new Point(0.5, -0.12),
                ArrowBasePoint1 = new Point(0.33, 0.5),
                ArrowBasePoint2 = new Point(0.67, 0.5)
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //UserSession.User = null;
            txtUsername.Focus();
        }

        private void ToggleCallout()
        {
            if (Keyboard.IsKeyToggled(Key.CapsLock))
            {
                CalloutPopupSettings calloutSettings = new CalloutPopupSettings()
                {
                    AutoClose = true,
                    ShowAnimationType = CalloutAnimation.FadeAndReveal,
                    ShowAnimationDuration = 0.5,
                    CloseAnimationDuration = 0.3,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                };
                CalloutPopupService.Show(_calloutCapslock, txtPassword, calloutSettings);
            }
            else
            {
                CalloutPopupService.Close(_calloutCapslock);
            }
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            CalloutPopupService.Close(_calloutCapslock);
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            ToggleCallout();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = Convert.ToBase64String(Encoding.UTF8.GetBytes(txtPassword.Password));
            User user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                UserSession.User = user;
                //DialogResult = true;
                MainWindow window = new MainWindow();
                window.Show();
                CalloutPopupService.CloseAll();
                this.Close();
            }
            else
            {
                CstMessageBox.Show("Login failed", "Incorrect username or password!", CstMessageBoxIcon.Error);
            }
        }

        private void txtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.CapsLock)
            {
                ToggleCallout();
            }
        }
    }
}

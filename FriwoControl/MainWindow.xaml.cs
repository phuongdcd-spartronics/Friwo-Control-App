using System;
using System.Windows;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            tbWelcome.Text = $"Welcome, {UserSession.User.FullName}";

            GrantPermission();
        }

        private void GrantPermission()
        {
            // Admin's functions
            mniSettings.Visibility = UserSession.HasRole(DBEnum.Roles.ADMIN) ? Visibility.Visible : Visibility.Collapsed;
            mniUserMgmt.Visibility = UserSession.HasRole(DBEnum.Roles.ADMIN) ? Visibility.Visible : Visibility.Collapsed;
            mniData.Visibility = UserSession.HasRole(DBEnum.Roles.ADMIN) ? Visibility.Visible : Visibility.Collapsed;

            // Operator's functions
            mniICTScan.Visibility = UserSession.HasRole(DBEnum.Roles.ICT) ? Visibility.Visible : Visibility.Collapsed;
            mniICTCheck.Visibility = UserSession.HasRole(DBEnum.Roles.ICT) ? Visibility.Visible : Visibility.Collapsed;
            mniVarnish.Visibility = UserSession.HasRole(DBEnum.Roles.VARNISH) ? Visibility.Visible : Visibility.Collapsed;
            mniBarcodeLink.Visibility = UserSession.HasRole(DBEnum.Roles.BARCODE_LINK) ? Visibility.Visible : Visibility.Collapsed;
            mniUnlinkBarcode.Visibility = UserSession.HasRole(DBEnum.Roles.BARCODE_UNLINK) ? Visibility.Visible : Visibility.Collapsed;
            mniUltraSonic.Visibility = UserSession.HasRole(DBEnum.Roles.ULTRA_SONIC) ? Visibility.Visible : Visibility.Collapsed;
            mniHiVoltage.Visibility = UserSession.HasRole(DBEnum.Roles.HI_VOLTAGE) ? Visibility.Visible : Visibility.Collapsed;
            mniATE.Visibility = UserSession.HasRole(DBEnum.Roles.ATE) ? Visibility.Visible : Visibility.Collapsed;
            mniFinal.Visibility = UserSession.HasRole(DBEnum.Roles.FINAL) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void mniICTScan_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/ICTScanPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "ICT";
        }

        private void mniICTCheck_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/ICTCheckPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "ICT Check";
        }

        private void mniAssemblySetup_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var window = new AssemblySetupWindow();
            window.ShowDialog();
        }

        private void mniWOSetup_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var window = new WOSetupWindow();
            window.ShowDialog();
        }

        private void mniAppSetup_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var window = new AppSetupWindow();
            window.ShowDialog();
        }

        private void mniLogOut_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            UserSession.User = null;
            AuthWindow window = new AuthWindow();
            window.Show();
            this.Close();
        }

        private void mniExit_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void mniVarnish_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/VarnishPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Varnish";
        }

        private void mniCloseAll_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Source = null;
            tbPage.Text = "Home";
        }

        private void mniUserMgmt_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/UserManagementPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "User Management";
        }

        private void mniChangePwd_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/ChangePasswordPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Change Password";
        }

        private void mniBarcodeLink_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/BarcodeLinkPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Barcode Link";
        }

        private void mniUltraSonic_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/UltraSonicPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "UltraSonic";
        }

        private void mniHiVoltage_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/HiVoltagePage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Hi-Voltage";
        }

        private void mniATE_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/AtePage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "ATE";
        }

        private void mniFinal_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/FinalStationPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Final";
        }

        private void mniManageSerial_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/BarcodeManagementPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Manage serial number";
        }

        private void mniUnlinkBarcode_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            frameContent.Navigate(new Uri("Pages/BarcodeUnlinkPage.xaml", UriKind.RelativeOrAbsolute));
            tbPage.Text = "Unlink Barcode";
        }
    }
}

using System;
using System.Windows;
using System.Windows.Forms;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for AppSetupWindow.xaml
    /// </summary>
    public partial class AppSetupWindow : Window
    {
        public AppSetupWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtICTFolder.Text = Properties.Settings.Default.ICT_Log_Folder;
            nudICTInterval.Value = Properties.Settings.Default.ICT_Log_Interval;
            nudICTInterval.Value = Properties.Settings.Default.ICT_Log_Interval;
            txtATEFolder.Text = Properties.Settings.Default.ATE_Log_Folder;
            nudATEInterval.Value = Properties.Settings.Default.ATE_Log_Interval;
            nudUltraClean.Value = Properties.Settings.Default.UltSonic_Clean_Max;
            cbbFinalPrinter.SelectedValue = Properties.Settings.Default.Box_Printer;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ICT_Log_Folder = txtICTFolder.Text;
            Properties.Settings.Default.ICT_Log_Interval = (int)nudICTInterval.Value;
            Properties.Settings.Default.ATE_Log_Folder = txtATEFolder.Text;
            Properties.Settings.Default.ATE_Log_Interval = (int)nudATEInterval.Value;
            Properties.Settings.Default.UltSonic_Clean_Max = (int)nudUltraClean.Value;
            Properties.Settings.Default.Box_Printer = cbbFinalPrinter.SelectedValue?.ToString();

            Properties.Settings.Default.Save();
            CstMessageBox.Show("Success", "All settings are saved successfully!", CstMessageBoxIcon.Success);
        }

        private void btnICTBrowse_Click(object sender, RoutedEventArgs e)
        {
            string folder = OpenFolderDialog();
            if (!String.IsNullOrEmpty(folder))
            {
                txtICTFolder.Text = folder;
            }
        }

        private string OpenFolderDialog()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return dialog.SelectedPath;
            return null;
        }

        private void btnATEBrowse_Click(object sender, RoutedEventArgs e)
        {
            string folder = OpenFolderDialog();
            if (!String.IsNullOrEmpty(folder))
            {
                txtATEFolder.Text = folder;
            }
        }
    }
}

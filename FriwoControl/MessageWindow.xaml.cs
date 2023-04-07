using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;
        }

        public MessageWindow(string title, string message, CstMessageBoxIcon icon) : this(title, message)
        {
            switch (icon)
            {
                case CstMessageBoxIcon.Success:
                    //txtIcon.Text = System.Web.HttpUtility.HtmlDecode("&#9989;");
                    txtIcon.Foreground = System.Windows.Media.Brushes.Green;
                    break;
                case CstMessageBoxIcon.Error:
                    //txtIcon.Text = System.Web.HttpUtility.HtmlDecode("&#9938;");
                    txtIcon.Foreground = System.Windows.Media.Brushes.Red;
                    break;
                case CstMessageBoxIcon.Warning:
                    //txtIcon.Text = System.Web.HttpUtility.HtmlDecode("&#9888;");
                    txtIcon.Foreground = System.Windows.Media.Brushes.Gold;
                    break;
                case CstMessageBoxIcon.Message:
                    //txtIcon.Text = System.Web.HttpUtility.HtmlDecode("&#9993;");
                    txtIcon.Foreground = System.Windows.Media.Brushes.DodgerBlue;
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void txtClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }

    public enum CstMessageBoxIcon
    {
        Success = 0,
        Error = 1,
        Warning = 2,
        Message = 3
    }

    public static class CstMessageBox
    {
        public static void Show(string title, string message)
        {
            MessageWindow window = new MessageWindow(title, message);
            window.ShowDialog();
        }

        public static void Show(string title, string message, CstMessageBoxIcon icon)
        {
            MessageWindow window = new MessageWindow(title, message, icon);
            window.ShowDialog();
        }
    }
}

using FriwoControl.Models;
using System;
using System.Windows;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for AssemblyInputWindow.xaml
    /// </summary>
    public partial class AssemblyInputWindow : Window
    {
        private string _type;
        public ProdAssembly ProdAssembly { get; private set; }

        public AssemblyInputWindow()
        {
            InitializeComponent();
            //_type = rdbGermany.Content.ToString();
            _type = String.Empty;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string qtyText = txtQty.Text;
            int qty = 0;

            // Alert when user leave empty input
            if (String.IsNullOrEmpty(name))
            {
                CstMessageBox.Show("Invalid input!", "Assembly name cannot be empty!", CstMessageBoxIcon.Error);
                return;
            }
            // User input invalid number
            else if (!Int32.TryParse(qtyText, out qty) || qty <= 0)
            {
                CstMessageBox.Show("Invalid number!", "Quantity must be greater than zero!", CstMessageBoxIcon.Error);
                return;
            } 
            else if (_type == String.Empty)
            {
                CstMessageBox.Show("Invalid product type!", "Please choose product type to continue!", CstMessageBoxIcon.Error);
                return;
            }

            ProdAssembly = new ProdAssembly()
            {
                Name = name,
                QtyPer = qty,
                Type = _type,
                CreatedAt = DateTime.Now,
                CreatedBy = UserSession.User.Username
            };

            DialogResult = true;
        }

        private void RadRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as Telerik.Windows.Controls.RadRadioButton;
            if (radioButton != null)
            {
                _type = radioButton.Content.ToString();
            }
        }
    }
}

using FriwoControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FriwoControl.UserControls
{
    /// <summary>
    /// Interaction logic for ucWorkInfo.xaml
    /// </summary>
    public partial class ucWorkInfo : UserControl
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();
        public ObservableCollection<ProductionOrder> ProductionList { get; private set; }
        public ObservableCollection<String> ShiftList { get; private set; }
        public ProductionOrder SelectedWO { get; private set; }
        public String SelectedShift { get; private set; }

        public event RoutedEventHandler Started;

        public ucWorkInfo()
        {
            InitializeComponent();
            
            ProductionList = new ObservableCollection<ProductionOrder>(
                // Exclude closed production orders
                _dbContext.ProductionOrders
                    .Where(p => p.ModifiedAt == null)
                    .OrderByDescending(p => p.CreatedAt));

            ShiftList = new ObservableCollection<string>(new List<string>()
            {
                "Office Hours / Hành chính",
                "Shift 1 / Ca 1",
                "Shift 2 / Ca 2",
                "Shift 3 / Ca 3",
                "Overtime / Tăng ca"
            });

            cbbWO.ItemsSource = ProductionList;

            this.DataContext = this;
        }

        public bool ValidateInput()
        {
            var prod = cbbWO.SelectedValue as ProductionOrder;
            string shift = cbbShift.SelectedValue?.ToString();
            if (prod == null)
            {
                CstMessageBox.Show("Invalid Work Order", "Please select Work Order!", CstMessageBoxIcon.Warning);
                return false;
            }
            else if (shift == null)
            {
                CstMessageBox.Show("Invalid Shift", "Please select your working shift!", CstMessageBoxIcon.Warning);
                return false;
            }

            SelectedWO = prod;
            SelectedShift = shift;

            return true;
        }

        public void Disable(bool isDisabled = true)
        {
            cbbWO.IsEnabled = !isDisabled;
            cbbShift.IsEnabled = !isDisabled;
            btnStart.IsEnabled = !isDisabled;
        }

        private void cbbWO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var prod = cbbWO.SelectedValue as ProductionOrder;
            if (prod != null)
            {
                txtAssembly.Text = prod.AssemblyName;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Disable(); 
                if (Started != null)
                {
                    Started.Invoke(this, new RoutedEventArgs());
                }
            }
        }
    }
}

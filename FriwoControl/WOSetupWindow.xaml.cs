using FriwoControl.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for WOSetupWindow.xaml
    /// </summary>
    public partial class WOSetupWindow : Window
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();
        public ObservableCollection<ProdAssembly> AssemblyList { get; set; }
        public ObservableCollection<ProductionOrder> ProductionList { get; set; }

        public WOSetupWindow()
        {
            InitializeComponent();

            AssemblyList = new ObservableCollection<ProdAssembly>(
                _dbContext.ProdAssemblies.AsNoTracking());
            ProductionList = new ObservableCollection<ProductionOrder>(
                _dbContext.ProductionOrders.AsNoTracking());

            this.DataContext = this;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var assembly = cbbAssembly.SelectedValue as ProdAssembly;
            if (assembly == null)
            {
                CstMessageBox.Show("Assembly not found", $"Please select assembly", CstMessageBoxIcon.Warning);
                return;
            }

            string wo = txtWO.Text;
            string quantity = txtQuantity.Text;

            var production = new ProductionOrder()
            {
                WO = wo,
                AssemblyName = assembly.Name,
                CreatedAt = DateTime.Now,
                CreatedBy = UserSession.User.Username,
                Quantity = decimal.Parse(quantity)
            };

            _dbContext.ProductionOrders.Add(production);
            if (_dbContext.SaveChanges() > 0)
            {
                ProductionList.Add(production);
                txtWO.Text = txtQuantity.Text = null;
                cbbAssembly.SelectedValue = null;
            }
        }

        private void gvProduction_DataLoaded(object sender, EventArgs e)
        {
            gvProduction.Columns[0].ColumnFilterDescriptor.FieldFilter.Filter1.Operator = Telerik.Windows.Data.FilterOperator.Contains;
            gvProduction.Columns[1].ColumnFilterDescriptor.FieldFilter.Filter1.Operator = Telerik.Windows.Data.FilterOperator.Contains;
        }

        private void gvProduction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveGridData();
            }
        }

        private void RemoveGridData()
        {
            var prod = gvProduction.SelectedItem as ProductionOrder;
            if (prod != null)
            {
                // Prevent delete WO has been running
                if (_dbContext.SerialRoutings.Any(s => s.WorkOrder == prod.WO))
                {
                    CstMessageBox.Show("Forbidden", $"This WO has been running. Cannot delete {prod.WO}", CstMessageBoxIcon.Error);
                    return;
                }

                _dbContext.ProductionOrders.Remove(prod);
                if (_dbContext.SaveChanges() > 0)
                {
                    ProductionList.Remove(prod);
                    CstMessageBox.Show("Success", $"Removed {prod.WO}", CstMessageBoxIcon.Success);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var prod = button?.CommandParameter as ProductionOrder;
            if (prod != null)
            {
                if (prod.ModifiedAt.HasValue)
                {
                    CstMessageBox.Show("Closed", $"{prod.WO} was closed!", CstMessageBoxIcon.Warning);
                }
                else
                {
                    var qty = _dbContext.BoxItems.Count(b => b.WO == prod.WO);
                    if (MessageBox.Show($"Are you sure to close \"{prod.WO}\"?\nTotal: {qty}/{prod.Quantity:#,#}", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        prod.ModifiedAt = DateTime.Now;
                        _dbContext.Entry(prod).State = EntityState.Modified;
                        if (_dbContext.SaveChanges() > 0)
                        {
                            gvProduction.Rebind();
                        }
                    }
                }
            }
        }
    }
}

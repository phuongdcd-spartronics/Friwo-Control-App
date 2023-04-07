using FriwoControl.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for BarcodeManagementPage.xaml
    /// </summary>
    public partial class BarcodeManagementPage : Page
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();
        public ObservableCollection<BarcodeInfo> BarcodeList { get; private set; }
        public ObservableCollection<SerialRouting> ProcessList { get; private set; }
        public ObservableCollection<SerialLinking> LinkBarcodeList { get; private set; }

        public BarcodeManagementPage()
        {
            InitializeComponent();

            BarcodeList = new ObservableCollection<BarcodeInfo>(
                _dbContext.BarcodeInfoes.OrderByDescending(b => b.CreatedAt));
            LinkBarcodeList = new ObservableCollection<SerialLinking>(
                _dbContext.SerialLinkings.ToList());
            ProcessList = new ObservableCollection<SerialRouting>();

            this.DataContext = this;
        }

        private void gvBarcode_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var barcode = gvBarcode.SelectedItem as BarcodeInfo;
            if (barcode != null)
            {
                ProcessList.Clear();
                foreach (SerialRouting route in _dbContext.SerialRoutings.Where(r => r.SerialNumber == barcode.Barcode).OrderBy(r => r.Step))
                {
                    ProcessList.Add(route);
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadButton;
            if (button != null)
            {
                var route = button.CommandParameter as SerialRouting;
                if (route != null && route.Status != DBEnum.Status.MISSING)
                {
                    // Prevent delete barcode with all routes are passed
                    if (!_dbContext.SerialRoutings.Any(s => s.SerialNumber == route.SerialNumber && s.Status != DBEnum.Status.PASS))
                    {
                        CstMessageBox.Show("Forbidden", $"This barcode has been done. Cannot reset {route.SerialNumber}!", CstMessageBoxIcon.Error);
                        return;
                    }

                    if (MessageBox.Show($"Are you sure to reset {route.Station} status?", "Reset", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        DateTime? cTimespan = route.Timespan;
                        string cStatus = route.Status;

                        route.Timespan = null;
                        route.Status = DBEnum.Status.MISSING;

                        // Save deleted history
                        _dbContext.Histories.Add(new History()
                        {
                            Action = "Reset",
                            Reference = $"{Environment.MachineName}$ResetStatus",
                            Description = $"Id: {route.Id}; SerialNumber: {route.SerialNumber}; Station: {route.Station}; Timespan: {cTimespan}; Status: {cStatus}",
                            CreatedAt = DateTime.Now,
                            CreatedBy = UserSession.User.Username
                        });

                        _dbContext.SaveChanges();
                        // Update grid data
                        gvBarcode_SelectionChanged(gvBarcode, null);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadButton;
            if (button != null)
            {
                var barcode = button.CommandParameter as BarcodeInfo;
                if (barcode != null)
                {
                    // Prevent delete barcode with all routes are passed
                    if (!_dbContext.SerialRoutings.Any(s => s.SerialNumber == barcode.Barcode && s.Status != DBEnum.Status.PASS))
                    {
                        CstMessageBox.Show("Forbidden", $"This barcode has been done. Cannot delete {barcode.Barcode}!", CstMessageBoxIcon.Error);
                        return;
                    }

                    gvBarcode.SelectedItem = barcode;
                    if (MessageBox.Show($"Are you sure to delete all data of {barcode.Barcode}?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using (var transaction = _dbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                string data = barcode.Barcode;
                                var routes = _dbContext.SerialRoutings.Where(r => r.SerialNumber == data);
                                var links = _dbContext.SerialLinkings.Where(l => l.InternalSerial == data);

                                // Remove all related data
                                _dbContext.SerialRoutings.RemoveRange(routes);
                                _dbContext.SerialLinkings.RemoveRange(links);
                                _dbContext.BarcodeInfoes.Remove(barcode);

                                // Save deleted history
                                _dbContext.Histories.Add(new History()
                                {
                                    Action = "Delete",
                                    Reference = $"{Environment.MachineName}$BarcodeInfo",
                                    Description = $"Barcode: {data}; routes: {String.Join(", ", routes.Select(r => $"{r.Station} : {r.Status}"))}; linked: {String.Join(", ", links.Select(l => $"{l.Station} : {l.CustomSerial}"))}",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = UserSession.User.Username
                                });

                                _dbContext.SaveChanges();
                                transaction.Commit();

                                // Update UI
                                ProcessList.Clear();
                                BarcodeList.Remove(barcode);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                //Logging.Error(ex, "Delete barcode error!");
                                CstMessageBox.Show("Delete error", ex.Message, CstMessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}

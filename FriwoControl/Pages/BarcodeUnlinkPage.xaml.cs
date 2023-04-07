using FriwoControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for BarcodeUnlinkPage.xaml
    /// </summary>
    public partial class BarcodeUnlinkPage : Page
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private MediaPlayer _player = new MediaPlayer();

        public ObservableCollection<SerialLinking> Links { get; set; }

        public BarcodeUnlinkPage()
        {
            InitializeComponent();

            _player.Open(new Uri(Properties.Settings.Default.Error_Sound_Uri, UriKind.RelativeOrAbsolute));
            _player.Volume = 1;
        }

        private void PlayErrorSound()
        {
            try
            {
                _player.Position = TimeSpan.FromSeconds(0);
                _player.Play();
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Play sound failed!");
            }
        }

        private void ucWorkInfo_Started(object sender, RoutedEventArgs e)
        {
            txtInBarcode.IsEnabled = true;
            txtCusBarcode.IsEnabled = true;
            txtInBarcode.Focus();

            Links = new ObservableCollection<SerialLinking>();
            this.DataContext = this;
        }

        private void txtInBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string barcode = txtInBarcode.Text.Trim();
                // Load related barcode
                Links.Clear();
                foreach (var link in _dbContext.SerialLinkings.Where(l => l.InternalSerial == barcode).AsNoTracking())
                {
                    Links.Add(link);
                }
                txtCusBarcode.Focus();
                ProcessData();
            }
        }

        private void txtCusBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtInBarcode.Focus();
                ProcessData();
            }
        }

        private void ProcessData()
        {
            string inBarcode = txtInBarcode.Text.Trim();
            string cusBarcode = txtCusBarcode.Text.Trim();

            if (!String.IsNullOrEmpty(cusBarcode) && !String.IsNullOrEmpty(inBarcode))
            {
                tbMessage.Text = null;

                try
                {
                    SerialLinking link = _dbContext.SerialLinkings.FirstOrDefault(s => s.InternalSerial == inBarcode && s.CustomSerial == cusBarcode);
                    if (link == null)
                        throw new Exception($"{inBarcode} và {cusBarcode} không được link với nhau");

                    // Reset the route status when unlinking a barcode
                    SerialRouting route = _dbContext.SerialRoutings.FirstOrDefault(s => s.SerialNumber == link.InternalSerial && s.Station == link.Station);

                    if (route == null)
                        throw new KeyNotFoundException($"Không tìm thấy {link.InternalSerial}");
                    // Prevent delete barcode with all routes are passed
                    else if (_dbContext.SerialRoutings.Any(s => s.SerialNumber == link.InternalSerial && s.Step > route.Step && s.Status != DBEnum.Status.MISSING))
                        throw new Exception($"Barcode này đã xử lý ở quy trình sau. Không thể xóa {link.InternalSerial}!");
                    else if (route.AssemblyName != ucWorkInfo.SelectedWO.AssemblyName)
                        throw new Exception($"{route.SerialNumber} không thuộc sản phẩm {ucWorkInfo.SelectedWO.AssemblyName}");

                    if (route != null)
                    {
                        route.Timespan = null;
                        route.Status = DBEnum.Status.MISSING;
                    }

                    _dbContext.SerialLinkings.Remove(link);
                    // Save deleted history
                    _dbContext.Histories.Add(new History()
                    {
                        Action = "Delete",
                        Reference = $"{Environment.MachineName}$UnlinkBarcode",
                        Description = $"InternalSerial: {link.InternalSerial}; CustomSerial: {link.CustomSerial}; Station: {link.Station}",
                        CreatedAt = DateTime.Now,
                        CreatedBy = UserSession.User.Username
                    });
                    _dbContext.SaveChanges();

                    tbMessage.Foreground = Brushes.Green;
                    tbMessage.Text = $"{link.InternalSerial} đã bỏ liên kết {link.CustomSerial}";
                }
                catch (Exception ex)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = ex.Message;
                    PlayErrorSound();
                }

                Links.Clear();
                txtInBarcode.Text = null;
                txtCusBarcode.Text = null;
                txtInBarcode.Focus();
            }
        }
    }
}

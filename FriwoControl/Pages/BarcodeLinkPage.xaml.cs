using FriwoControl.Models;
using FriwoControl.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for BarcodeLinkPage.xaml
    /// </summary>
    public partial class BarcodeLinkPage : Page
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private MediaPlayer _player = new MediaPlayer();

        public ObservableCollection<SerialRouting> Logs { get; private set; }
        public String Station { get; private set; } = DBEnum.Processes.BARCODE_LINK;

        public BarcodeLinkPage()
        {
            InitializeComponent();

            _player.Open(new Uri(Properties.Settings.Default.Error_Sound_Uri, UriKind.RelativeOrAbsolute));
            _player.Volume = 1;
        }

        private void UpdateQuantity()
        {
            ucGridStatus.UpdateQuantity(ucWorkInfo.SelectedWO.WO, Station);
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
            var prod = ucWorkInfo.SelectedWO;
            var logs = _dbContext.SerialRoutings.Where(r => r.WorkOrder == prod.WO && r.Station == Station)
                .OrderByDescending(r => r.Timespan).ToList();
            Logs = new ObservableCollection<SerialRouting>(logs);

            this.DataContext = this;
            txtInBarcode.IsEnabled = true;
            txtCusBarcode.IsEnabled = true;
            txtInBarcode.Focus();
            UpdateQuantity();
        }

        private void txtInBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string barcode = txtInBarcode.Text.Trim();
                BarcodeInfo barcodeInfo = _dbContext.BarcodeInfoes.FirstOrDefault(bi => bi.Barcode == barcode);
                SerialRouting current = null;
                SerialLinking serialLink = _dbContext.SerialLinkings.FirstOrDefault(l => l.InternalSerial == barcode && l.Station == Station);
                try
                {
                    // Validation before processing data
                    //if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, barcode, Station, DBEnum.Status.PASS, out current, false))
                    if (serialLink != null)
                    {
                        throw new Exception($"{barcode} đang được link với {serialLink.CustomSerial}! Cần unlink trước khi xử lý!");
                    }
                    else if (barcodeInfo != null && barcodeInfo.WorkOrder != ucWorkInfo.SelectedWO.WO)
                    {
                        throw new Exception($"`{barcode}` thuộc WO#{barcodeInfo.WorkOrder} khác WO#{ucWorkInfo.SelectedWO.WO} đã chọn");
                    }
                    else
                    {
                        txtCusBarcode.Focus();
                        ProcessData();
                    }
                }
                catch (Exception ex)
                {
                    //Logging.Error(ex, "Validate internal barcode failed!");
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = ex.Message;
                    PlayErrorSound();
                    txtInBarcode.Text = null;
                    txtInBarcode.Focus();
                }
            }
        }

        private void txtCusBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string barcode = txtCusBarcode.Text.Trim();
                SerialLinking serialLink = _dbContext.SerialLinkings.FirstOrDefault(x => x.CustomSerial == barcode);
                // Retrieve WO from barcode
                string wo = barcode.Length > 10 ? barcode.Substring(barcode.Length - 10, 5) : barcode;

                // Check correct format pattern for barcode
                if (!Regex.IsMatch(barcode, BarcodeUtils.BARCODE_LINK_PATTERN))
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = $"`{barcode}` sai định dạng nhãn liên kết";
                    txtCusBarcode.Text = null;
                    PlayErrorSound();
                    return;
                }
                // Check for selected WO and linked barcode WO
                else if (ucWorkInfo.SelectedWO.WO != wo)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = $"`{barcode}` thuộc WO#{wo} khác WO#{ucWorkInfo.SelectedWO.WO} đã chọn";
                    txtCusBarcode.Text = null;
                    PlayErrorSound();
                    return;
                }
                // Warn to user if barcode is already used
                else if (serialLink != null)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = $"`{barcode}` đã sử dụng để liên kết `{serialLink.InternalSerial}` tại trạm {serialLink.Station}";
                    txtInBarcode.Text = null;
                    txtCusBarcode.Text = null;
                    txtInBarcode.Focus();
                    PlayErrorSound();
                    return;
                }
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
                    SerialRouting current = null;
                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, inBarcode, Station, DBEnum.Status.PASS, out current))
                    {
                        // Save the linked barcode
                        _dbContext.SerialLinkings.Add(new SerialLinking()
                        {
                            InternalSerial = inBarcode,
                            CustomSerial = cusBarcode,
                            Station = Station
                        });
                        _dbContext.SaveChanges();

                        tbMessage.Foreground = Brushes.Green;
                        tbMessage.Text = $"Passed {Station}. {inBarcode} đã liên kết với {cusBarcode}!";
                        Logs.Insert(0, current);
                        UpdateQuantity();
                    }
                }
                catch (Exception ex)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = ex.Message;
                    PlayErrorSound();
                }

                txtInBarcode.Text = null;
                txtCusBarcode.Text = null;
                txtInBarcode.Focus();
            }
        }
    }
}

using FriwoControl.Models;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for UltraSonicPage.xaml
    /// </summary>
    public partial class UltraSonicPage : Page
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private readonly int CLEAN_LIMIT;
        private int _cleanQty = 0;

        public ObservableCollection<SerialRouting> Logs { get; private set; }
        public String Station { get; private set; } = DBEnum.Processes.ULTRASONIC;

        public UltraSonicPage()
        {
            InitializeComponent();

            CLEAN_LIMIT = Properties.Settings.Default.UltSonic_Clean_Max;
            _cleanQty = Properties.Settings.Default.UltSonic_Clean_Quantity;
            CleanUpChecker();
        }

        private void UpdateQuantity()
        {
            Properties.Settings.Default.UltSonic_Clean_Quantity = ++_cleanQty;
            Properties.Settings.Default.Save();
            ucGridStatus.UpdateQuantity(ucWorkInfo.SelectedWO.WO, Station);
            CleanUpChecker();
        }

        private void ucWorkInfo_Started(object sender, RoutedEventArgs e)
        {
            var prod = ucWorkInfo.SelectedWO;
            var logs = _dbContext.SerialRoutings.Where(r => r.WorkOrder == prod.WO && r.Station == Station)
                .OrderByDescending(r => r.Timespan).ToList();
            Logs = new ObservableCollection<SerialRouting>(logs);

            this.DataContext = this;
            txtBarcode.IsEnabled = true;
            txtBarcode.Focus();
            UpdateQuantity();
        }

        private void TriggerMachine()
        {
            using (SerialPort _serialPort = new SerialPort("COM9"))
            {
                try
                {
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.Parity = Parity.None;
                    _serialPort.DataBits = 8;
                    _serialPort.BaudRate = 38400;
                    _serialPort.RtsEnable = true;
                    _serialPort.DtrEnable = true;
                    _serialPort.Open();
                    // Delay 1s
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    //Logging.Error(ex, "Trigger failed!");
                }
                finally
                {
                    if (_serialPort != null && _serialPort.IsOpen)
                        _serialPort.Close();
                }
            }
        }

        private void CleanUpChecker()
        {
            if (CLEAN_LIMIT > 0 && _cleanQty >= CLEAN_LIMIT)
            {
                MessageWindow window = new MessageWindow("Vệ sinh khuôn", $"Thiết bị đã được sử dụng quá {CLEAN_LIMIT} lần! Cần vệ sinh khuôn thiết bị để tiếp tục sử dụng!", CstMessageBoxIcon.Warning);
                window.btnConfirm.Content = "Hoàn tất";
                // Hide the page if user closes the warning
                window.txtClose.MouseDown += (o, e) =>
                {
                    this.Visibility = Visibility.Hidden;
                };
                window.btnConfirm.Click += (o, e) =>
                {
                    try
                    {
                        // Record history after clicked confirm button
                        _dbContext.Histories.Add(new History()
                        {
                            Action = "Clean",
                            Reference = $"{Environment.MachineName}${Station}",
                            Description = $"Clean up machine after {_cleanQty} usages",
                            CreatedBy = UserSession.User.Username,
                            CreatedAt = DateTime.Now
                        });
                        _dbContext.SaveChanges();
                        _cleanQty = 0;
                        // Reset quantity in settings
                        Properties.Settings.Default.UltSonic_Clean_Quantity = 0;
                        Properties.Settings.Default.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Lỗi lưu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                // Display alert center of the screen
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowDialog();
            }
        }

        private void txtBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            string text = txtBarcode.Text.Trim();
            if (!String.IsNullOrEmpty(text) && e.Key == Key.Enter)
            {
                tbMessage.Text = null;
                Properties.Settings.Default.UltSonic_Clean_Quantity = ++_cleanQty;
                Properties.Settings.Default.Save();
                CleanUpChecker();

                try
                {
                    SerialRouting current = null;
                    // Station is covered by linked barcode
                    // Get original serial number from custom serial number
                    SerialLinking link = _dbContext.SerialLinkings.FirstOrDefault(l => l.CustomSerial == text);
                    if (link == null)
                        throw new Exception($"{text} không được tìm thấy!");

                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, link.InternalSerial, Station, DBEnum.Status.PASS, out current))
                    {
                        tbMessage.Foreground = Brushes.Green;
                        tbMessage.Text = $"{link.InternalSerial} passed {Station}!";
                        Logs.Insert(0, current);
                        // Trigger to run the test machine
                        TriggerMachine();
                        UpdateQuantity();
                    }
                }
                catch (Exception ex)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = ex.Message;
                }

                txtBarcode.Text = null;
                txtBarcode.Focus();
            }
        }
    }
}

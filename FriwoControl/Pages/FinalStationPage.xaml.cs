using FriwoControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using FriwoControl.Utilities;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for FinalStationPage.xaml
    /// </summary>
    public partial class FinalStationPage : Page
    {
        private string CACHE_FILE_PATH = "cache_final.txt";

        private FriwoControlContext _dbContext = new FriwoControlContext();
        private IDictionary<string, DateTime> _serialDict = new Dictionary<string, DateTime>();
        private int _total = 0;
        private MediaPlayer _fPlayer = new MediaPlayer();
        private MediaPlayer _sPlayer = new MediaPlayer();

        public ObservableCollection<BoxItemLog> Logs { get; private set; }
        public ObservableCollection<String> SerialList { get; private set; }
        public String Station { get; private set; } = DBEnum.Processes.FINAL;

        public class BoxItemLog
        {
            public string Package { get; set; }
            public string WO { get; set; }
            public string SerialNumber { get; set; }
            public DateTime PrintedAt { get; set; }
        }

        public FinalStationPage()
        {
            InitializeComponent();
        }

        private void InitCachePath(string prefix)
        {
            string dir = FileUtils.GetAppDataPath("cache");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            CACHE_FILE_PATH = Path.Combine(dir, $"{prefix}_final.txt");
        }

        private void PlayErrorSound()
        {
            try
            {
                _fPlayer.Position = TimeSpan.FromSeconds(0);
                _fPlayer.Play();
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Play sound failed!");
            }
        }

        private void PlaySuccessSound()
        {
            try
            {
                _sPlayer.Position = TimeSpan.FromSeconds(0);
                _sPlayer.Play();
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Play sound failed!");
            }
        }

        private void ucWorkInfo_Started(object sender, RoutedEventArgs e)
        {
            var prod = ucWorkInfo.SelectedWO;
            var logs = (from b in _dbContext.BoxItems
                        join s in _dbContext.SerialLinkings on b.SerialNumber equals s.InternalSerial
                        where b.WO == prod.WO
                        orderby b.PrintedAt descending
                        select new BoxItemLog()
                        {
                            Package = b.Package,
                            WO = b.WO,
                            SerialNumber = s.CustomSerial,
                            PrintedAt = b.PrintedAt
                        })
                        .ToList();
            var assembly = _dbContext.ProdAssemblies.FirstOrDefault(a => a.Name == prod.AssemblyName);

            Logs = new ObservableCollection<BoxItemLog>(logs);
            SerialList = new ObservableCollection<string>();
            _total = assembly.QtyPer;
            // Read cached data from file
            InitCachePath(prod.WO);
            ReadFromCache();
            try
            {
                // Initialize error sound when application throw exception
                _fPlayer.Open(new Uri(Properties.Settings.Default.Error_Sound_Uri, UriKind.RelativeOrAbsolute));
                _fPlayer.Volume = 1;
                // Initialize success sound when package has enough quantity
                _sPlayer.Open(new Uri(Properties.Settings.Default.Success_Sound_Uri, UriKind.RelativeOrAbsolute));
                _sPlayer.Volume = 1;
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Load sound failed!");
            }

            this.DataContext = this;
            txtBarcode.IsEnabled = true;
            txtBarcode.Focus();
            UpdateUI();
        }

        private void UpdateUI()
        {
            tbQuantity.Text = SerialList.Count.ToString("#,0");
            tbTotal.Text = _total.ToString("#,0");
        }

        private void ClearData()
        {
            // Clear data and update UI
            SerialList.Clear();
            _serialDict.Clear();
            // Delete cached file
            if (File.Exists(CACHE_FILE_PATH))
                File.Delete(CACHE_FILE_PATH);
            UpdateUI();
        }

        private void ReadFromCache()
        {
            if (File.Exists(CACHE_FILE_PATH))
            {
                try
                {
                    var lines = File.ReadAllLines(CACHE_FILE_PATH);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2)
                            throw new Exception($"Wrong format for {line}");
                        string serial = parts[0];
                        DateTime timespan = DateTime.Parse(parts[1]);
                        SerialList.Add(serial);
                        _serialDict.Add(serial, timespan);
                    }
                    UpdateUI();
                }
                catch (Exception ex)
                {
                    ClearData();
                    //Logging.Error(ex, $"Read from cache failed!");
                }
            }
        }

        private void WriteToCache(string text, DateTime timespan)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(CACHE_FILE_PATH))
                {
                    sw.WriteLine($"{text}\t{timespan}");
                }
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, $"Cached failed for {text} : {timespan}");
            }
        }

        private void lbSerial_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Delete)
            {
                if (MessageBox.Show("Clear all data?", "Clear", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ClearData();
                }
            }
            else if (e.Key == Key.Delete)
            {
                var serial = lbSerial.SelectedItem?.ToString();
                if (MessageBox.Show($"Remove {serial}?", "Remove", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    SerialList.Remove(serial);
                    _serialDict.Remove(serial);
                    UpdateUI();
                }
            }
        }

        private void PrintLabel(string serial)
        {
            var label = new Labels.BoxLabel();
            try
            {
                label.Print(serial);
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Print label failed!");
            }
            finally
            {
                label.Close();
            }
        }

        private void UpdateProcessAndPrintLabel()
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    string wo = ucWorkInfo.SelectedWO.WO;
                    string assembly = ucWorkInfo.SelectedWO.AssemblyName;
                    var boxes = _dbContext.BoxItems.Where(b => b.WO == wo);
                    // ID of the box for each production order and started with 1
                    int boxId = boxes.Any() ? (boxes.Max(b => b.SeqNo) + 1) : 1;
                    string package = $"{(wo.Length > 5 ? wo.Substring(wo.Length - 5, 5) : wo)}-{boxId.ToString().PadLeft(4, '0')}";
                    BoxItem boxItem = null;

                    // Fetch a list of serial number to save into a package with ID Box
                    foreach (string serial in SerialList)
                    {
                        SerialRouting route = _dbContext.SerialRoutings.FirstOrDefault(s => s.SerialNumber == serial && s.Station == DBEnum.Processes.FINAL);
                        if (route == null)
                            throw new Exception($"Could not find {serial} in database!");

                        DateTime timespan = _serialDict.ContainsKey(serial) ? _serialDict[serial] : DateTime.Now;

                        route.WorkOrder = wo;
                        route.AssemblyName = assembly;
                        route.Shift = ucWorkInfo.SelectedShift;
                        route.Status = DBEnum.Status.PASS;
                        route.Timespan = timespan;

                        // Add to history
                        _dbContext.Histories.Add(new History()
                        {
                            Action = "Update",
                            Reference = $"{Environment.MachineName}${Station}",
                            Description = $"Updated {route.SerialNumber} at {Station}: [{route.Status}] (WO: {route.WorkOrder}; Shift: {route.Shift})",
                            CreatedBy = UserSession.User.Username,
                            CreatedAt = DateTime.Now
                        });
                        _dbContext.SaveChanges();

                        boxItem = new BoxItem()
                        {
                            Package = package,
                            SerialNumber = serial,
                            WO = wo,
                            AssemblyName = assembly,
                            SeqNo = boxId,
                            PrintedAt = timespan,
                            Machine = $"{Environment.MachineName}"
                        };
                        // Add to box record
                        _dbContext.BoxItems.Add(boxItem);
                        _dbContext.SaveChanges();

                        //Logging.Info($"Update {route.SerialNumber} with status {route.Status} at {route.Station}.");
                    }

                    transaction.Commit();
                    if (boxItem != null)
                    {
                        var serial = _dbContext.SerialLinkings.FirstOrDefault(l => l.InternalSerial == boxItem.SerialNumber);
                        Logs.Insert(0, new BoxItemLog()
                        {
                            Package = boxItem.Package,
                            WO = boxItem.WO,
                            SerialNumber = serial?.CustomSerial ?? boxItem.SerialNumber,
                            PrintedAt = boxItem.PrintedAt
                        });
                    }

                    // Print package label
                    PrintLabel(package);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    //Logging.Error(ex, "Failed to print label at final station!");
                    throw ex;
                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (SerialList != null && SerialList.Count > 0)
            {
                if (MessageBox.Show($"Bạn chắc chắn muốn xuất phiếu lẻ cho {SerialList.Count} sản phẩm?", "Xuất phiếu lẻ", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    UpdateProcessAndPrintLabel();
                    ClearData();
                }
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            // Only admin can re-print label
            if (!UserSession.HasRole(DBEnum.Roles.ADMIN))
            {
                CstMessageBox.Show("Access denied", $"Please contact administrator to active this function!", CstMessageBoxIcon.Error);
                return;
            }

            var button = sender as RadButton;
            if (button != null)
            {
                var box = button.CommandParameter as BoxItemLog;
                if (box != null)
                {
                    PrintLabel(box.Package);
                    //Logging.Info($"{UserSession.User.FullName} ({UserSession.User.Username}) printed label {box.Package}");
                }
            }
        }

        private void txtBarcode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            string text = txtBarcode.Text.Trim();
            if (!String.IsNullOrEmpty(text) && e.Key == Key.Enter)
            {
                tbMessage.Text = null;

                try
                {
                    SerialRouting current = null;
                    // Station is covered by linked barcode
                    // Get original barcode from custom serial
                    SerialLinking link = _dbContext.SerialLinkings
                        .FirstOrDefault(l => l.CustomSerial == text);
                    if (link == null)
                        throw new Exception($"{text} không được tìm thấy!");
                    else if (link.BarcodeInfo.WorkOrder != ucWorkInfo.SelectedWO.WO)
                        throw new DataMisalignedException($"WO của {link.CustomSerial} ({link.BarcodeInfo.WorkOrder}) không thuộc WO đã chọn!");

                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, link.InternalSerial, Station, DBEnum.Status.PASS, out current, false))
                    {
                        if (SerialList.Contains(link.InternalSerial))
                        {
                            throw new DuplicateNameException($"{link.InternalSerial} đã được thêm vào rồi!");
                        }
                        else
                        {
                            var box = _dbContext.BoxItems.FirstOrDefault(b => b.SerialNumber == current.SerialNumber);
                            if (box != null)
                            {
                                throw new DuplicateNameException($"{link.CustomSerial} đã được sử dụng cho thùng {box.Package}");
                            }
                        }

                        SerialList.Insert(0, link.InternalSerial);
                        // Record the added time span
                        _serialDict.Add(link.InternalSerial, DateTime.Now);
                        WriteToCache(link.InternalSerial, DateTime.Now);
                        UpdateUI();

                        // Quantity is enough for a package
                        if (SerialList.Count == _total)
                        {
                            PlaySuccessSound();
                            UpdateProcessAndPrintLabel();
                            ClearData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    tbMessage.Foreground = Brushes.Red;
                    tbMessage.Text = ex.Message;
                    PlayErrorSound();
                }

                txtBarcode.Text = null;
                txtBarcode.Focus();
            }
        }
    }
}

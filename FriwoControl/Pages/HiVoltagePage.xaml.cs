using FriwoControl.Chroma;
using FriwoControl.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for HiVoltagePage.xaml
    /// </summary>
    public partial class HiVoltagePage : Page
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private MediaPlayer _player = new MediaPlayer();
        private Chroma_1920 _chroma = new Chroma_1920();

        public ObservableCollection<SerialRouting> Logs { get; private set; }
        public String Station { get; private set; } = DBEnum.Processes.HI_VOLTAGE;

        public HiVoltagePage()
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

        private bool? TriggerMachine(out string current)
        {
            current = null;
            if (_chroma.Run())
            {
                var result = _chroma.Read_Result(2);
                if (result.status)
                {
                    current = result.Current;
                    return result.result == "PASSED";
                }
            }

            return null;
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

            // Connect test machine
            if (!_chroma.GetId(9))
            {
                tbMessage.Foreground = Brushes.Gold;
                tbMessage.Text = "Không thể kết nối tới thiết bị test tại COM9";
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
                    // Get original serial number from custom serial number
                    SerialLinking link = _dbContext.SerialLinkings.FirstOrDefault(l => l.CustomSerial == text);
                    if (link == null)
                        throw new Exception($"{text} không được tìm thấy!");

                    // Current result from test machine
                    string testCurrent = null;
                    string status = DBEnum.Status.PASS;
                    bool? result = null;
                    // Validate process before trigger machine
                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, link.InternalSerial, Station, status, out current, false))
                    {
                        result = TriggerMachine(out testCurrent);
                        if (result != null)
                            status = result.Value ? DBEnum.Status.PASS : DBEnum.Status.FAIL;
                    }

                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, link.InternalSerial, Station, status, out current))
                    {
                        // Cannot connect to machine or something wrong in the test process
                        if (result == null)
                        {
                            tbMessage.Foreground = Brushes.Gold;
                            tbMessage.Text = $"{text} passed {Station} nhưng không kết nối được thiết bị. Khởi động bằng tay để test!";
                        }
                        else
                        {
                            // Record tested result
                            if (!String.IsNullOrEmpty(testCurrent))
                            {
                                _dbContext.Histories.Add(new History()
                                {
                                    Action = "Chroma",
                                    Reference = $"{Environment.MachineName}$HighVoltageRecord",
                                    Description = testCurrent,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = UserSession.User.Username
                                });
                                _dbContext.SaveChanges();
                            }

                            tbMessage.Foreground = Brushes.Green;
                            tbMessage.Text = $"{link.InternalSerial} passed {Station}!";
                        }
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

                txtBarcode.Text = null;
                txtBarcode.Focus();
            }
        }
    }
}

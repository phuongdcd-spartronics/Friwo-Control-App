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
    /// Interaction logic for ICTCheckPage.xaml
    /// </summary>
    public partial class ICTCheckPage : Page
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();
        private MediaPlayer _player = new MediaPlayer();

        public ObservableCollection<SerialRouting> Logs { get; private set; }
        public String Station { get; private set; } = DBEnum.Processes.ICT_CHECK;

        public ICTCheckPage()
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
            txtBarcode.IsEnabled = true;
            txtBarcode.Focus();
            UpdateQuantity();
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
                    if (ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, text, Station, DBEnum.Status.PASS, out current))
                    {
                        tbMessage.Foreground = Brushes.Green;
                        tbMessage.Text = $"{text} passed {Station}!";
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

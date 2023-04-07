using FriwoControl.Models;
using FriwoControl.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for AtePage.xaml
    /// </summary>
    public partial class AtePage : Page
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();
        private DispatcherTimer _timer = new DispatcherTimer();
        private List<String> _readLine = new List<string>();
        public String Station { get; private set; } = DBEnum.Processes.ATE;

        public AtePage()
        {
            InitializeComponent();
            txtPath.Text = Properties.Settings.Default.ATE_Log_Folder;
            _timer.Tick += (o, e) =>
            {
                ReadFiles();
            };
        }

        private void UpdateQuantity()
        {
            ucGridStatus.UpdateQuantity(ucWorkInfo.SelectedWO.WO, Station);
        }

        private void BrowseDirectory(string root, List<string> directories)
        {
            var dirs = Directory.GetDirectories(root);
            foreach (string dir in dirs)
            {
                // Read the folders started with the name of current assembly
                if (Path.GetFileName(dir).StartsWith(ucWorkInfo.SelectedWO.AssemblyName))
                    directories.Add(dir);
            }
        }

        private void ReadFiles()
        {
            string root = Properties.Settings.Default.ATE_Log_Folder;
            List<string> dirs = new List<string>();
            // Get directories to browse files
            BrowseDirectory(root, dirs);

            foreach (string dir in dirs)
            {
                foreach (string filePath in Directory.GetFiles(dir))
                {
                    // Read all files on today
                    if (Path.GetFileNameWithoutExtension(filePath).EndsWith($"{DateTime.Now:yyyyMMdd}"))
                    {
                        var lines = FileUtils.WriteSafeReadAllLines(filePath);
                        foreach (var line in lines)
                        {
                            try
                            {
                                // The lines start with SN are the data to be read
                                if (line.StartsWith("SN"))
                                {
                                    // Split each data column of the line
                                    string[] lineParts = line.Split('\t');
                                    // The data to be read must contain at least 3 data columns
                                    if (lineParts.Length > 3)
                                    {
                                        // The serial number part in the 1st column
                                        // The result in the 2nd column
                                        // The date in the 3rd column
                                        string[] serialParts = lineParts[0].Split(':');
                                        if (serialParts.Length > 1)
                                        {
                                            string serial = serialParts[1];
                                            string result = lineParts[1].ToUpper();
                                            DateTime date = DateTime.Parse(lineParts[2]);
                                            // Update record result if it isn't read before
                                            if (!_dbContext.ATE_Record.Any(r => r.SerialNumber == serial && r.Date == date))
                                            {
                                                // Station is covered by linked barcode
                                                // Get original barcode from custom serial
                                                SerialLinking link = _dbContext.SerialLinkings
                                                    .FirstOrDefault(l => l.CustomSerial == serial);
                                                if (link != null)
                                                {
                                                    string status = result.ToUpper() == "PASS" ? DBEnum.Status.PASS : DBEnum.Status.FAIL;
                                                    SerialRouting route = null;
                                                    // Update current process status
                                                    ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, link.BarcodeInfo.Barcode, Station, status, out route);
                                                    // Save the record in order to ignore other
                                                    _dbContext.ATE_Record.Add(new ATE_Record()
                                                    {
                                                        SerialNumber = serial,
                                                        Result = result,
                                                        Date = date,
                                                        CreatedAt = DateTime.Now,
                                                        CreatedBy = UserSession.User.Username
                                                    });
                                                    _dbContext.SaveChanges();
                                                    // Logging information
                                                    string logMessage = $"Read successfully {serial} | {result} | {date} from {filePath}!";
                                                    //Logging.Info(logMessage);
                                                    AddLog(logMessage, Brushes.Green);
                                                    UpdateQuantity();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Add to read path in order to not log again
                                if (!_readLine.Contains(line))
                                {
                                    //Logging.Error(ex, $"Failed to read {filePath}");
                                    AddLog($"{ex.Message}. Failed to read {filePath}", Brushes.Red);
                                    _readLine.Add(line);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddLog(string text, Brush color = null)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Foreground = color ?? Brushes.Black;
            paragraph.Inlines.Add(new Run($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t"));
            paragraph.Inlines.Add(new Run(text));
            rtxtLog.Document.Blocks.Add(paragraph);
            rtxtLog.Document.Blocks.Add(new Paragraph(new Run("--------------------------------\n")));
            if (chkScroll.IsChecked == true)
                rtxtLog.ScrollToEnd();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (ucWorkInfo.ValidateInput())
            {
                UpdateQuantity();

                // Start read log folder
                if (!_timer.IsEnabled)
                {
                    string root = txtPath.Text;
                    if (!Directory.Exists(root))
                    {
                        CstMessageBox.Show("Invalid directory", $"Cannot find directory {root}");
                        return;
                    }

                    Properties.Settings.Default.ICT_Log_Folder = txtPath.Text;
                    Properties.Settings.Default.Save();
                    _timer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.ATE_Log_Interval);
                    _timer.Start();
                    ucWorkInfo.Disable(true);
                    btnStart.Content = "Stop";
                    btnStart.Background = Brushes.Red;
                    AddLog("Started interval!", Brushes.Green);
                }
                // Stop read log folder
                else
                {
                    _timer.Stop();
                    ucWorkInfo.Disable(false);
                    btnStart.Content = "Start";
                    btnStart.Background = Brushes.LightGreen;
                    AddLog("Stopped interval!", Brushes.Red);
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            rtxtLog.Document.Blocks.Clear();
        }

        private void ucWorkInfo_Started(object sender, RoutedEventArgs e)
        {
            btnStart_Click(btnStart, e);
        }
    }
}

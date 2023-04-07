using FriwoControl.Models;
using FriwoControl.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace FriwoControl.Pages
{
    /// <summary>
    /// Interaction logic for ICTScanPage.xaml
    /// </summary>
    public partial class ICTScanPage : Page
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();
        private DispatcherTimer _timer = new DispatcherTimer();
        private List<string> _readPath = new List<string>();
        private string _productType = null;
        public String Station { get; private set; } = DBEnum.Processes.ICT;

        public ICTScanPage()
        {
            InitializeComponent();
            txtPath.Text = Properties.Settings.Default.ICT_Log_Folder;
            _timer.Tick += (o, e) =>
            {
                ReadFiles();
            };
        }

        private void UpdateQuantity()
        {
            ucGridStatus.UpdateQuantity(ucWorkInfo.SelectedWO.WO, Station);
        }

        private void BrowseDirectory(string root, string filterName, List<string> directories)
        {
            var dirs = Directory.GetDirectories(root);
            filterName = filterName.ToUpper();
            foreach (string dir in dirs)
            {
                if (root.ToUpper().Contains(filterName) && Path.GetFileName(dir) == "REP")
                {
                    directories.Add(dir);
                }
                BrowseDirectory(dir, filterName, directories);
            }
        }

        private void ReadFiles()
        {
            string root = Properties.Settings.Default.ICT_Log_Folder;
            string assembly = ucWorkInfo.SelectedWO.AssemblyName;
            string wo = ucWorkInfo.SelectedWO.WO;
            List<string> dirs = new List<string>();
            BrowseDirectory(root, assembly, dirs);

            foreach (string dir in dirs)
            {
                string[] folders = new string[] { "PASS", "FAIL" };
                foreach (string folder in folders)
                {
                    string sourceFolder = Path.Combine(dir, folder);
                    string backupFolder = Path.Combine(dir, $"{folder}_{DateTime.Now:yyyy_MM_dd}");
                    string failedFolder = Path.Combine(dir, $"{folder}_Error", $"{DateTime.Now:yyyy_MM_dd}");

                    // Read all files if the directory is existing
                    if (Directory.Exists(sourceFolder))
                    {
                        foreach (string filePath in Directory.GetFiles(sourceFolder))
                        {
                            string barcode = Path.GetFileNameWithoutExtension(filePath);
                            if (BarcodeUtils.ValidateBarcodeInfo(barcode, _productType, assembly, wo))
                            {
                                Task.Run(() =>
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        int affected = 0;
                                        try
                                        {
                                            if (BarcodeUtils.ValidateBarcode(barcode, _productType))
                                            {
                                                string fileName = Path.GetFileName(filePath);
                                                string[] lines = File.ReadAllLines(filePath);
                                                bool handled = false;
                                                foreach (string line in lines)
                                                {
                                                    string data = line.ToUpper();
                                                    // Read the line had the result
                                                    if (data.Contains("RESULT"))
                                                    {
                                                        string[] resultParts = data.Split(new string[] { "RESULT" }, StringSplitOptions.RemoveEmptyEntries);
                                                        if (resultParts.Length < 2)
                                                        {
                                                            //Logging.Info($"Failed to read result from file ${filePath}");
                                                            AddLog($"Failed to read result from file {filePath}!", Brushes.Red);
                                                        }
                                                        else
                                                        {
                                                            // Read the result part, result failed if it has F character
                                                            string status = resultParts[1].Contains("F") ? DBEnum.Status.FAIL : DBEnum.Status.PASS;
                                                            SerialRouting current = null;
                                                            // Update current process status
                                                            ucGridStatus.UpdateProcess(ucWorkInfo.SelectedWO, ucWorkInfo.SelectedShift, barcode, DBEnum.Processes.ICT, status, out current);
                                                            // Logging information
                                                            //Logging.Info($"Read successfully from {filePath}!");
                                                            AddLog($"{barcode}!", Brushes.Green);
                                                            // Set handled in order to move the file to other place
                                                            handled = true;
                                                            affected++;
                                                        }
                                                    }
                                                }

                                                // Move the file to other folder if it is read
                                                if (handled)
                                                {
                                                    if (!Directory.Exists(backupFolder))
                                                        Directory.CreateDirectory(backupFolder);
                                                    string cutPath = Path.Combine(backupFolder, fileName);
                                                    if (File.Exists(cutPath))
                                                    {
                                                        string ext = Path.GetFileNameWithoutExtension(fileName);
                                                        // Append timestamp at the end of existing file
                                                        File.Move(cutPath, cutPath.Replace(ext, $"{DateTime.Now:HHmmss}{ext}"));
                                                    }
                                                    File.Move(filePath, cutPath);
                                                }
                                            }
                                            else
                                            {
                                                throw new FormatException($"{barcode} is not in pattern!");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Add to read path in order to not log again
                                            if (!_readPath.Contains(filePath))
                                            {
                                                //Logging.Error(ex, $"Failed to read {filePath}");
                                                AddLog($"{ex.Message}", Brushes.Red);
                                                _readPath.Add(filePath);
                                                // Move to the error folder to prevent from reading again
                                                if (!Directory.Exists(failedFolder))
                                                    Directory.CreateDirectory(failedFolder);
                                                string errorPath = Path.Combine(failedFolder, Path.GetFileName(filePath));
                                                File.Move(filePath, errorPath);
                                            }
                                        }

                                        // Update quantity if data were changed
                                        if (affected > 0)
                                            UpdateQuantity();
                                    });
                                });
                            }
                        }
                    }
                }
            }
        }

        private void AddLog(string text, Brush color = null)
        {
            // Clear the logs if it exceeds specified lines
            if (rtxtLog.Document.Blocks.Count > 50)
            {
                rtxtLog.Document.Blocks.Clear();
            }
            Paragraph paragraph = new Paragraph();
            paragraph.Foreground = color ?? Brushes.Black;
            paragraph.Inlines.Add(new Run($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t"));
            paragraph.Inlines.Add(new Run(text));
            rtxtLog.Document.Blocks.Add(paragraph);
            //rtxtLog.Document.Blocks.Add(new Paragraph(new Run("--------------------------------\n")));
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
                    _timer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.ICT_Log_Interval);
                    _timer.Start();
                    _productType = _dbContext.ProdAssemblies
                        .FirstOrDefault(p => p.Name == ucWorkInfo.SelectedWO.AssemblyName)?.Type;
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

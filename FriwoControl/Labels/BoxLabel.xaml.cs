using FriwoControl.Utilities;
using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;

namespace FriwoControl.Labels
{
    /// <summary>
    /// Interaction logic for BoxLabel.xaml
    /// </summary>
    public partial class BoxLabel : Window
    {
        public BoxLabel()
        {
            InitializeComponent();
        }

        public void Print(string serial)
        {
            string printerName = Properties.Settings.Default.Box_Printer;

            // call print function
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    brcSerial.Source = BarcodeUtils.GetQRCodeImage(serial);

                    PrintDialog printer = new PrintDialog();
                    if (!String.IsNullOrEmpty(printerName))
                    {
                        printer.PrintQueue = new LocalPrintServer().GetPrintQueue(printerName);
                    }

                    printer.PrintVisual(this.printArea, $"{serial} - Friwo Box Label");
                }
                catch (Exception ex)
                {
                    CstMessageBox.Show("Print error", ex.Message, CstMessageBoxIcon.Error);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}

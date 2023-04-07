using QRCoder;
using System;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace FriwoControl.Utilities
{
    public class BarcodeUtils
    {
        // Barcode pattern for Germany: Assembly-WO-SN-Rev
        public const string BARCODE_DE_PATTERN = @"^[a-zA-Z0-9_]+-\w{5}-\w{5}-\w{1,3}$";
        // Barcode pattern for Vietnam: Assembly-SN-WO[S]-Rev
        public const string BARCODE_VN_PATTERN = @"^[a-zA-Z0-9_]+-\w{7}-\w{5}S-\w{1,3}$";
        // Link barcode pattern: AssemblyWOSN
        public const string BARCODE_LINK_PATTERN = @"^[a-zA-Z0-9_]+\w{5}\w{5}$";

        public static bool ValidateBarcode(string barcode, string type)
        {
            if (type == "Germany")
            {
                return Regex.IsMatch(barcode, BARCODE_DE_PATTERN);
            }
            else if (type == "Vietnam")
            {
                return Regex.IsMatch(barcode, BARCODE_VN_PATTERN);
            }

            return false;
        }

        public static bool ValidateBarcodeInfo(string barcode, string type, string assembly, string wo)
        {
            string pattern = String.Empty;
            bool moreInfo = !String.IsNullOrEmpty(assembly) && !String.IsNullOrEmpty(wo);

            if (type == "Germany")
            {
                if (moreInfo)
                {
                    pattern = assembly + "-" + wo + @"-\w{5}-\w{1,3}$";
                }
                else
                {
                    pattern = BARCODE_DE_PATTERN;
                }
            }
            else if (type == "Vietnam")
            {
                if (moreInfo)
                {
                    pattern = assembly + @"-\w{7}-" + wo + @"-\w{1,3}$";
                }
                else
                {
                    pattern = BARCODE_VN_PATTERN;
                }
            }

            return Regex.IsMatch(barcode, pattern);
        }

        public static BitmapImage GetQRCodeImage(string data)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                QRCodeData qrdata = qRCodeGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
                QRCode qrcode = new QRCode(qrdata);
                System.Drawing.Bitmap qrimage = qrcode.GetGraphic(28);
                var ms = new System.IO.MemoryStream();
                qrimage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                image.BeginInit();
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Print error!");
                throw new Exception("QRCode generated error!");
            }

            return image;
        }
    }
}

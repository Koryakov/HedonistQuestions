using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hedonist.Wpf.Helpers {
    internal class AppSettingsHelper {
        public static Settings Settings { get; set; } = new Settings();
        static AppSettingsHelper() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            Settings = config.GetRequiredSection("Settings").Get<Settings>();
        }
    }

    internal class BitmapHelper {
        private static byte[] CreateQrCodeByteArray(string qrCodeText) {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        public static BitmapImage CreateQrCodeBitmap(string qrCodeText) {
            byte[] qrCodeByteArray = CreateQrCodeByteArray(qrCodeText);

            //using (var ms = new MemoryStream(giftDataResponse.qrCodeData.QrCodeByteArr)) {
            using (var ms = new MemoryStream(qrCodeByteArray)) {
                ms.Position = 0;
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.EndInit();
                return imageSource;

                //imgQrCode.Source = imageSource;
                //txtQrCode.Text = giftDataResponse.qrCodeData.QrCodeText;
            }
        }
    }
}

using Hedonist.Models;
using ModalControl;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Imaging;

namespace Hedonist.Wpf {

    public class GiftWorker
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
      
        public static BitmapImage GetImageSource(string imgRelativePath, string imgFileName) {
            try {
                string imgPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, imgRelativePath, imgFileName);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imgPath);
                bitmapImage.EndInit();

                return bitmapImage;
            } catch (Exception ex) {
                logger.Error(ex, $"GiftWorker.GetImageControl() EXCEPTION. imgRelativePath={imgRelativePath}, imgFileName={imgFileName}");
                return new BitmapImage();
            }
        }

        
    }

}

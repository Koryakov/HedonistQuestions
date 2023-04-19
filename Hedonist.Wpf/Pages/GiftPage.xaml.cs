using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for GiftPage.xaml
    /// </summary>
    public partial class GiftPage : Page {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string backgroundImageName;
        private static List<GiftTypeRelation> GiftTypeRelations { get; set; } = new();
        public string BackgroundImageSource { get; set; }
        static GiftPage() {
           
            try {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
                GiftTypeRelations = config.GetRequiredSection("GiftTypeRelation").Get<List<GiftTypeRelation>>();
                logger.Info($"static GiftPage() completed;");
            }
            catch (Exception ex) {
                logger.Error(ex, "static GiftPage() Exception;");

            }
        }
        public GiftPage(GiftPageModel model) {
            InitializeComponent();
            GiftTypeRelation selectedRelation = GiftTypeRelations[0];

            string imgPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"Images\\Back", selectedRelation.BgImageName);
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(imgPath);
            myBitmapImage.EndInit();
            imgBackground.Source = myBitmapImage;
        }
    }
}

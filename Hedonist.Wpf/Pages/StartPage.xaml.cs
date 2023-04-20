using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public StartPage() {
            logger.Info("StartPage() constructor called");
            InitializeComponent();
            hypnoVideo.Play();
            hypnoVideo.MediaEnded += HypnoVideo_MediaEnded;
        }

        private void HypnoVideo_MediaEnded(object sender, RoutedEventArgs e) {
            hypnoVideo.Position = TimeSpan.Zero;
            hypnoVideo.Play();
        }

        private void GridClick(object sender, MouseButtonEventArgs e) {
            var mouseWasDownOn = e.Source as FrameworkElement;
            if (mouseWasDownOn.Tag == null || mouseWasDownOn.Tag.ToString() != "LockButton") {
                NavigationService.Navigate(new InfoPage());
            }
        }
        private void LockButtonClick(object sender, MouseButtonEventArgs e) {
            var mouseWasDownOn = e.Source as FrameworkElement;
            if (mouseWasDownOn.Tag.ToString() == "LockButton") {
                NavigationService.Navigate(new NumberPage());
            }
        }
    }
}

using Hedonist.Models;
using Hedonist.Wpf.Pages.GiftPages;
using ModalControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for HandPage.xaml
    /// </summary>
    public partial class HandPage : Page {
        private string ticket;
        private GiftType giftType;

        public HandPage(string ticket, GiftType giftType) {
            this.ticket = ticket;
            this.giftType = giftType;

            InitializeComponent();

            grdBeforeScan.Visibility = Visibility.Visible;
            grdInScanProcess.Visibility = Visibility.Hidden;
            grdAfterScan.Visibility = Visibility.Hidden;

            palmScannerVideo.MediaEnded += PalmScannerVideo_MediaEnded;
        }

        private void PalmScannerVideo_MediaEnded(object sender, RoutedEventArgs e) {
            grdBeforeScan.Visibility = Visibility.Hidden;
            grdInScanProcess.Visibility = Visibility.Hidden;
            grdAfterScan.Visibility = Visibility.Visible;
        }

        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new GiftPage1(ticket, giftType));
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }

        private void Path_MouseDown(object sender, MouseButtonEventArgs e) {
            grdBeforeScan.Visibility = Visibility.Hidden;
            grdInScanProcess.Visibility = Visibility.Visible;
            grdAfterScan.Visibility = Visibility.Hidden;
            palmScannerVideo.Play();
        }
    }
}

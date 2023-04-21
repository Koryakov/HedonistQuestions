using Hedonist.Models;
using Hedonist.Wpf.Pages.GiftPages;
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
    /// Interaction logic for Variants.xaml
    /// </summary>
    public partial class VariantsPage : Page {
        private string ticket;
        private GiftCommonData GiftData { get; set; }
        public VariantsPage(string ticket, GiftCommonData giftData) {
            this.ticket = ticket;
            this.GiftData = giftData;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            string btnName = ((Button)sender).Name as string;
            switch (btnName) {
                case "btnArt":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnMusic":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnTrends":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnFood":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnMovement":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnMixology":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
                case "btnCommunication":
                    NavigationService.Navigate(new GiftPage1(ticket, GiftData));
                    break;
            }
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }
    }
}

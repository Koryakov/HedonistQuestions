using Hedonist.Models;
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

namespace Hedonist.Wpf.Pages.GiftPages {
    /// <summary>
    /// Interaction logic for GiftsOver.xaml
    /// </summary>
    public partial class GiftsOver : Page {
        public class GiftsOverModel {
            public String HeaderText { get; set; }
            public string Ticket { get; set; }
        }

        private GiftsOverModel model;
        private GiftCommonData GiftData { get; set; }
        private const string TextBlock1Pattern = "{0},\r\nно уверены, что ты знаешь лучше.";
        
        public GiftsOver(GiftsOverModel model, GiftCommonData giftData) {
            InitializeComponent();
            this.model = model;
            this.GiftData = giftData;
            txtWeThink.Text = string.Format(TextBlock1Pattern, model.HeaderText);
        }
        private void btnAgain_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new TestPage(model.Ticket));
        }

        private void btnSayWhoAmI_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new VariantsPage(model.Ticket, GiftData));
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }
    }
}

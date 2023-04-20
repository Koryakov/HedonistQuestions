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

namespace Hedonist.Wpf.Pages.GiftPages
{
    /// <summary>
    /// Interaction logic for GiftMusicPage_1.xaml
    /// </summary>
    public partial class GiftMusicPage_1 : Page
    {
        private TestPageModel giftPageModel;
        public GiftMusicPage_1(TestPageModel giftPageModel)
        {
            this.giftPageModel = giftPageModel;
            InitializeComponent();
        }

        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new GiftMusicPage_3(giftPageModel));
        }
        private void btnOneMore_Click(object sender, RoutedEventArgs e) {

        }
        private void btnChoose_Click(object sender, RoutedEventArgs e) {
        }
    }
}

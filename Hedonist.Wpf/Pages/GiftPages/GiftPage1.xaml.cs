using Hedonist.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    /// Interaction logic for GiftPage1.xaml
    /// </summary>
    public partial class GiftPage1 : Page {

        private GiftCommonData GiftData { get; set; }
        public GiftPage1(string ticket, GiftCommonData giftData) {
            this.GiftData = giftData;
            InitializeComponent();
           
            dynamic jsonData = JsonConvert.DeserializeObject(GiftData.GiftType.ExtendedData);
            if (Exists(jsonData, "GiftPage1Data")) {
                var giftPage1Data = jsonData.GiftPage1Data;
                if (Exists(giftData, "Text1")) {
                    txtDescription.Text = giftPage1Data.Text1;
                }
            }
        }
        public static bool Exists(dynamic settings, string name) {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }


        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            //NavigationService.Navigate(new GiftMusicPage_3(giftPageModel));
        }
        private void btnOneMore_Click(object sender, RoutedEventArgs e) {

        }
        private void btnChoose_Click(object sender, RoutedEventArgs e) {
        }
    }
}

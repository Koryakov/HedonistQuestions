using Hedonist.Models;
using Hedonist.Wpf.Helpers;
using ModalControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private JObject exData;
        private int pageState = 0;
        private string ticket;
        private JToken giftPage1Data;
        public GiftPage1(string ticket, GiftCommonData giftData) {

            this.ticket = ticket;
            this.GiftData = giftData;
            exData = JObject.Parse(GiftData.GiftType.ExtendedData);

            InitializeComponent();
            Bind();
        }

        private void Bind() {
            switch(pageState) {
                case 0:
                    BindPage1();
                    pageState = 1;
                    break;
                case 1:
                    BindPage2(); 
                    pageState = 2;
                    break;
            }
        }

        private void BindPage1() {
            try {
                giftPage1Data = exData["GiftPage1Data"];

                string bgImageFileName = giftPage1Data["bgImageName"].ToString();
                imgBackground.Source = GiftWorker.GetImageSource($"Images\\Back", bgImageFileName);

                txtHeader1.Text = giftPage1Data["Text1"].ToString();
                txtHeader2.Text = giftPage1Data["Text2"].ToString();
                txtDescription1.Text = giftPage1Data["Text3"].ToString();
                txtDescription2.Text = giftPage1Data["Text4"].ToString();

                panelStoreGift.Visibility = Visibility.Hidden;
                panelQrCode.Visibility = Visibility.Hidden;
                panelStart.Visibility = Visibility.Visible;
            } catch (Exception ex) {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
        }

        private void BindPage2() {
            try {
                if (GiftData.GiftResult == GiftCommonData.GiftResultType.GiftFound) {

                    if (GiftData.GiftType.HasQrCode) {
                        txtQrCode.Text = GiftData.QrCodeText;
                        imgQrCode.Source = BitmapHelper.CreateQrCodeBitmap(txtQrCode.Text);

                        panelStoreGift.Visibility = Visibility.Hidden;
                        panelQrCode.Visibility = Visibility.Visible;
                        panelStart.Visibility = Visibility.Hidden;
                    }
                    else {
                        panelStoreGift.Visibility = Visibility.Visible;
                        panelQrCode.Visibility = Visibility.Hidden;
                        panelStart.Visibility = Visibility.Hidden;
                    }
                }
                else if (GiftData.GiftResult == GiftCommonData.GiftResultType.NoFreeGift) {
                    var model = new GiftsOver.GiftsOverModel() {
                        HeaderText = giftPage1Data["GiftOverText"].ToString(),
                        Ticket = ticket
                    };
                    NavigationService.Navigate(new GiftsOver(model, GiftData));
                }
                else {
                    modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                    modal.IsOpen = true;
                }
            }
            catch (Exception ex) {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
        }

        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            Bind();
        }
        private void btnOneMore_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new TestPage(ticket));
        }
        private void btnChoose_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new VariantsPage(ticket, GiftData));
        }
        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }
    }
}

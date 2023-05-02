using Hedonist.Models;
using Hedonist.Wpf.Helpers;
using ModalControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private GiftType GiftTypeFromTestPage { get; set; }
        private JObject exData;
        private string ticket;
        private JToken giftPage1Data;
        private BackgroundWorker bgWorkerGift = new();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private (AutorizeResultType resultType, GiftCommonData giftCommonData) giftDataResponseFromDb;

        public GiftPage1(string ticket, GiftType giftType) {
            try {
                InitializeComponent();

                this.ticket = ticket;
                
                this.GiftTypeFromTestPage = giftType;
                if (giftType == null) {
                    modalMessage.Text = "Терминал не зарегистрирован в системе";
                    modal.IsOpen = true;

                    return;
                }
                exData = JObject.Parse(giftType.ExtendedData);

                BindPage1();
            }
            catch (Exception ex) {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
        }

      
        private void BgWorkerGift_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN BgWorkerGift_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorkerGift_DoWork() Task Run()...");
                    giftDataResponseFromDb = await ClientEngine.GetGiftByTypeAsync(ticket, GiftTypeFromTestPage.Id);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorkerGift_DoWork; resetEvent.Wait() ended");

            }
            catch (TaskCanceledException ex) {
                giftDataResponseFromDb = new(AutorizeResultType.Timeout, null);
                logger.Error(ex, "BgWorkerGift_DoWork() TIMEOUT");
            }
            catch (Exception ex) {
                giftDataResponseFromDb = new(AutorizeResultType.Error, null);
                logger.Error(ex, "BgWorkerGift_DoWork() EXCEPTION");
            }
        }

        private void BgWorkerGift_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug($"IN BgWorkerGift_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (giftDataResponseFromDb.resultType == AutorizeResultType.Authorized) {

                BindPage3();
            }
            else {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BgWorkerGift_RunWorkerCompleted()");
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
                panelStoreGift.Visibility = Visibility.Hidden;
                panelAreYoShure.Visibility = Visibility.Visible;
                panelQrCode.Visibility = Visibility.Hidden;
                panelStart.Visibility = Visibility.Hidden;
            }
            catch (Exception ex) {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
        }

        private void BindPage3() {
            try {
                var giftCommonData = giftDataResponseFromDb.giftCommonData;

                if (giftCommonData.GiftResult == GiftCommonData.GiftResultType.GiftFound) {

                    if (giftCommonData.GiftType.HasQrCode) {
                        txtQrCode.Text = giftCommonData.QrCodeText;
                        imgQrCode.Source = BitmapHelper.CreateQrCodeBitmap(txtQrCode.Text);

                        panelStoreGift.Visibility = Visibility.Hidden;
                        panelAreYoShure.Visibility = Visibility.Hidden;
                        panelQrCode.Visibility = Visibility.Visible;
                        panelStart.Visibility = Visibility.Hidden;
                    }
                    else {
                        panelStoreGift.Visibility = Visibility.Visible;
                        panelAreYoShure.Visibility = Visibility.Hidden;
                        panelQrCode.Visibility = Visibility.Hidden;
                        panelStart.Visibility = Visibility.Hidden;
                    }
                }
                else if (giftCommonData.GiftResult == GiftCommonData.GiftResultType.NoFreeGift
                    || giftCommonData.GiftResult == GiftCommonData.GiftResultType.StoreHasNoGiftType) {
                    var model = new GiftsOver.GiftsOverModel() {
                        HeaderText = giftPage1Data["GiftOverText"].ToString(),
                        Ticket = ticket
                    };
                    NavigationService.Navigate(new GiftsOver(model));
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
            BindPage2();
        }
        private void btnOneMore_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new TestPage(ticket));
        }
        private void btnChoose_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new VariantsPage(ticket));
        }
        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }

        private void btnYesIAmShure_Click(object sender, RoutedEventArgs e) {
            bgWorkerGift = new();
            bgWorkerGift.DoWork += BgWorkerGift_DoWork;
            bgWorkerGift.RunWorkerCompleted += BgWorkerGift_RunWorkerCompleted;

            if (!bgWorkerGift.IsBusy) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorkerGift.RunWorkerAsync()...");
                bgWorkerGift.RunWorkerAsync();
            }
        }
    }
}

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
            logger.Debug($"IN GiftPage1() constructor, ticket={ticket}, giftType is null ={giftType == null}");
            try {
                InitializeComponent();
                this.ticket = ticket;
                this.GiftTypeFromTestPage = giftType;
                
                if (giftType == null) {
                    logger.Warn($"GiftPage1() constructor - Terminal not registered");
                    modalMessage.Text = "Терминал не зарегистрирован в системе";
                    modal.IsOpen = true;

                    return;
                }
                logger.Debug($"GiftPage1() constructor, ticket={ticket}, giftType={giftType.Id}, name={giftType.Name}");
                exData = JObject.Parse(giftType.ExtendedData);

                BindPageState1();
            }
            catch (Exception ex) {
                logger.Error(ex, $"EXCEPTION IN GiftPage1() constructor");
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

                BindPageState3();
            }
            else {
                logger.Error($"BgWorkerGift_RunWorkerCompleted() something wrong. resultType={giftDataResponseFromDb.resultType}");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BgWorkerGift_RunWorkerCompleted()");
        }

        private void BindPageState1() {
            logger.Debug($"IN BindPageState1()");
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
                logger.Error(ex, $"EXCEPTION IN BindPageState1() constructor");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BindPageState1()");
        }

        private void BindPageState2() {
            logger.Debug($"IN BindPageState2()");
            try {
                panelStoreGift.Visibility = Visibility.Hidden;
                panelAreYoShure.Visibility = Visibility.Visible;
                panelQrCode.Visibility = Visibility.Hidden;
                panelStart.Visibility = Visibility.Hidden;
            }
            catch (Exception ex) {
                logger.Error(ex, $"EXCEPTION IN BindPageState2() constructor");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BindPageState2()");
        }

        private void BindPageState3() {
            logger.Debug($"IN BindPageState3()");
            try {
                var giftCommonData = giftDataResponseFromDb.giftCommonData;
                logger.Debug($"IN BindPageState3() giftCommonData.GiftResult={giftCommonData.GiftResult}");

                if (giftCommonData.GiftResult == GiftCommonData.GiftResultType.GiftFound) {
                    logger.Debug($"BindPageState3() GiftFound");

                    if (giftCommonData.GiftType.HasQrCode) {

                        txtQrCode.Text = giftCommonData.QrCodeText;
                        imgQrCode.Source = BitmapHelper.CreateQrCodeBitmap(txtQrCode.Text);

                        panelStoreGift.Visibility = Visibility.Hidden;
                        panelAreYoShure.Visibility = Visibility.Hidden;
                        panelQrCode.Visibility = Visibility.Visible;
                        panelStart.Visibility = Visibility.Hidden;

                        logger.Debug($"BindAfterPalmScan() HasQrCode, Gift Code was shown!");
                    }
                    else {
                        logger.Debug($"BindPageState3() NO HasQrCode, Gift was shown!");
                        panelStoreGift.Visibility = Visibility.Visible;
                        panelAreYoShure.Visibility = Visibility.Hidden;
                        panelQrCode.Visibility = Visibility.Hidden;
                        panelStart.Visibility = Visibility.Hidden;
                    }
                }
                else if (giftCommonData.GiftResult == GiftCommonData.GiftResultType.NoFreeGift
                    || giftCommonData.GiftResult == GiftCommonData.GiftResultType.StoreHasNoGiftType
#warning Handling InconsistentData is temporary solution for old backend comatibility
                    || giftCommonData.GiftResult == GiftCommonData.GiftResultType.InconsistentData
                    ) {
                    var model = new GiftsOver.GiftsOverModel() {
                        HeaderText = giftPage1Data["GiftOverText"].ToString(),
                        Ticket = ticket
                    };
                    logger.Debug($"OUT BindPageState3() Navigate(new GiftsOver(model)");
                    NavigationService.Navigate(new GiftsOver(model));
                }
                else {
                    logger.Error($"BindPageState3() something wrong. giftCommonData.GiftResult={giftCommonData.GiftResult}");
                    modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                    modal.IsOpen = true;
                }
            }
            catch (Exception ex) {
                logger.Error(ex, $"EXCEPTION IN BindPageState3() constructor");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BindPageState3()");
        }

        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            logger.Debug($"btnShowResult_Click()");
            BindPageState2();
        }
        private void btnOneMore_Click(object sender, RoutedEventArgs e) {
            logger.Debug($"btnOneMore_Click() Navigate(new TestPage(ticket={ticket})");
            NavigationService.Navigate(new TestPage(ticket));
        }
        private void btnChoose_Click(object sender, RoutedEventArgs e) {
            logger.Debug($"btnChoose_Click() Navigate(new VariantsPage(ticket={ticket})");
            NavigationService.Navigate(new VariantsPage(ticket));
        }
        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            logger.Debug($"OnCloseModalClick()");
            modal.IsOpen = false;
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            logger.Debug($"HomeButtonClick()");
            NavigationService.Navigate(new StartPage());
        }

        private void btnYesIAmShure_Click(object sender, RoutedEventArgs e) {
            logger.Debug($"btnYesIAmShure_Click()");
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

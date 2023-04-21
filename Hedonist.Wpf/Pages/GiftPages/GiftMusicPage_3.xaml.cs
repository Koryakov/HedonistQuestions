using Hedonist.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using static Hedonist.Wpf.Pages.TestPage;

namespace Hedonist.Wpf.Pages.GiftPages
{
    /// <summary>
    /// Interaction logic for GiftMusicPage_3.xaml
    /// </summary>
    public partial class GiftMusicPage_3 : Page
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private BackgroundWorker bgWorker = new();
        private GiftWorker giftWorker;
        private TestPageModel giftPageModel;
        private (AutorizeResultType resultType, GiftCommonData qrCodeData) giftDataResponse;

        public GiftMusicPage_3(TestPageModel giftPageModel)
        {
            this.giftPageModel = giftPageModel;
            logger.Debug($"IN GiftMusicPage_3() constructor");
            giftWorker = new GiftWorker("GiftMusicPage_3", giftPageModel);
            InitializeComponent();

            gridQrCodeBlock.Visibility = Visibility.Hidden;
            gridStoreGiftBlock.Visibility = Visibility.Hidden;

            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;

            if (!bgWorker.IsBusy) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorker.RunWorkerAsync()...");
                bgWorker.RunWorkerAsync();
            }
            logger.Debug("OUT GiftMusicPage_3() constructor");
        }

        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e) {
            giftWorker.DoWork();
        }

        private void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            spinner.IsLoading = false;

            GiftQrCodeCompleteData qrCompletedData;
            if (giftWorker.ProcessObtainedGiftData(out qrCompletedData)) {
                if (qrCompletedData.RawData.GiftResult == GiftCommonData.GiftResultType.NoFreeGift) {

                    var model = new GiftsOver.GiftsOverModel() {
                        HeaderText = "музыке",
                        Ticket = giftPageModel.Ticket
                    };
                    //NavigationService.Navigate(new GiftsOver(model));
                }
                else {
                    //QR код подарка
                    if (qrCompletedData.RawData.GiftType.HasQrCode) {
                        imgQrCode.Source = qrCompletedData.QrCodeImageSource;
                        txtQrCode.Text = qrCompletedData.RawData.QrCodeText;

                        gridQrCodeBlock.Visibility = Visibility.Visible;
                        gridStoreGiftBlock.Visibility = Visibility.Hidden;
                    }
                    //Подарок на точке
                    else {
                        gridQrCodeBlock.Visibility = Visibility.Hidden;
                        gridStoreGiftBlock.Visibility = Visibility.Visible;
                    }
                }
            }
            else {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug("OUT BgWorker_RunWorkerCompleted()");
        }
     
        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }
    }
}

using Hedonist.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool isErrorHappens = false;
        private GiftPageModel giftPageModel;
        private (AutorizeResultType resultType, HedonistGiftQrCodeData qrCodeData) giftDataResponse;

        public GiftMusicPage_3(GiftPageModel giftPageModel)
        {
            this.giftPageModel = giftPageModel;
            logger.Debug($"IN GiftMusicPage_3() constructor");
            InitializeComponent();
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
            try {
                logger.Debug("IN BgWorker_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorker_DoWork() Task Run()...");
                    giftDataResponse = await ClientEngine.GetGiftAsync(giftPageModel.Ticket, giftPageModel.SelectedAnswers.Last().Id);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorker_DoWork; resetEvent.Wait() ended");

            }
            catch (Exception ex) {
                isErrorHappens = true;
                logger.Error(ex, "BgWorker_DoWork() EXCEPTION");
            }
        }

        private void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug("IN BgWorker_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (isErrorHappens) {
                logger.Error("BgWorker_RunWorkerCompleted() isErrorHappens=TRUE");
                isErrorHappens = false;
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            else {
                switch (giftDataResponse.resultType) {
                    case AutorizeResultType.Authorized:
                        InitGiftScreenInformation();
                        break;
                    default:
                        modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                        modal.IsOpen = true;
                        break;
                }
            }
            logger.Debug("OUT BgWorker_RunWorkerCompleted()");
        }

        private void InitGiftScreenInformation() {
            using (var ms = new MemoryStream(giftDataResponse.qrCodeData.QrCodeByteArr)) {

                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.EndInit();

                // Assign the Source property of your image
                imgQrCode.Source = imageSource;
            }
        }

        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }
    }
}

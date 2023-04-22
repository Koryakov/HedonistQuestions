using Hedonist.Models;
using Hedonist.Wpf.Helpers;
using Hedonist.Wpf.Pages.GiftPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for Variants.xaml
    /// </summary>
    public partial class VariantsPage : Page {
        private string ticket;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private (AutorizeResultType resultType, GiftCommonData giftData) giftDataResponse;
        private (AutorizeResultType resultType, Store store) storeResponse;
        private BackgroundWorker bgWorkerStore = new();
        private BackgroundWorker bgWorkerGift = new();
        private int selectedGiftTypeId;
        public VariantsPage(string ticket) {
            this.ticket = ticket;
            InitializeComponent();

            bgWorkerStore = new();
            bgWorkerStore.DoWork += BgWorkerStore_DoWork;
            bgWorkerStore.RunWorkerCompleted += BgWorkerStore_RunWorkerCompleted;

            if (!bgWorkerStore.IsBusy) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorkerStore.RunWorkerAsync()...");
                bgWorkerStore.RunWorkerAsync();
            }
        }

        private void BgWorkerStore_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN BgWorkerStore_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorkerStore_DoWork() Task Run()...");
                    storeResponse = await ClientEngine.GetStoreAsync(ticket, AppSettingsHelper.Settings.TerminalIdentifier);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorkerStore_DoWork; resetEvent.Wait() ended");

            }
            catch (TaskCanceledException ex) {
                storeResponse = new(AutorizeResultType.Timeout, null);
                logger.Error(ex, "BgWorkerStore_DoWork() TIMEOUT");
            }
            catch (Exception ex) {
                storeResponse = new(AutorizeResultType.Error, null);
                logger.Error(ex, "BgWorkerStore_DoWork() EXCEPTION");
            }
        }

        private void BgWorkerStore_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug($"IN BgWorkerGiftType_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (storeResponse.resultType == AutorizeResultType.Authorized) {

            }
            else {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BgWorkerGiftType_RunWorkerCompleted()");
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            string btnName = ((Button)sender).Name as string;
            switch (btnName) {
                case "btnArt":
                    selectedGiftTypeId = 1;
                    break;
                case "btnMusic":
                    selectedGiftTypeId = 2;
                    break;
                case "btnTrends":
                    selectedGiftTypeId = 3;
                    break;
                case "btnFood":
                    int foodGiftTypeId = 41;
                    var giftType = storeResponse.store.GiftTypes.FirstOrDefault(x => x.Id == 41 || x.Id == 42);
                    if(giftType != null) {
                        foodGiftTypeId = giftType.Id;
                    }
                    selectedGiftTypeId = foodGiftTypeId;
                    break;
                case "btnMovement":
                    selectedGiftTypeId = 5;
                    break;
                case "btnMixology":
                    selectedGiftTypeId = 6;
                    break;
                case "btnCommunication":
                    selectedGiftTypeId = 7;
                    break;
            }
            RunGettingGiftData();
        }

        private void RunGettingGiftData() {
            bgWorkerGift = new();
            bgWorkerGift.DoWork += BgWorkerGift_DoWork;
            bgWorkerGift.RunWorkerCompleted += BgWorkerGift_RunWorkerCompleted;

            if (!bgWorkerGift.IsBusy) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorkerGift.RunWorkerAsync()...");
                bgWorkerGift.RunWorkerAsync();
            }
        }

        private void BgWorkerGift_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN BgWorkerGiftType_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorkerGiftType_DoWork() Task Run()...");
                    giftDataResponse = await ClientEngine.
                        GetGiftByTypeAsync(ticket, selectedGiftTypeId);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorkerGiftType_DoWork; resetEvent.Wait() ended");

            }
            catch (TaskCanceledException ex) {
                giftDataResponse = new(AutorizeResultType.Timeout, null);
                logger.Error(ex, "BgWorkerGiftType_DoWork() TIMEOUT");
            }
            catch (Exception ex) {
                giftDataResponse = new(AutorizeResultType.Error, null);
                logger.Error(ex, "BgWorkerGiftType_DoWork() EXCEPTION");
            }
        }


        private void BgWorkerGift_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug($"IN BgWorkerGiftType_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (giftDataResponse.resultType == AutorizeResultType.Authorized) {

                NavigationService.Navigate(new GiftPage1(ticket, giftDataResponse.giftData));
            }
            else {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BgWorkerGiftType_RunWorkerCompleted()");
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }

        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }
    }
}

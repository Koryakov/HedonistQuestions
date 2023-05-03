using Hedonist.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Shell;
using System.Windows.Threading;

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for Number.xaml
    /// </summary>
    public partial class NumberPage : Page {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly BackgroundWorker bgWorker = new BackgroundWorker();
        private AutorizeResult autorizeResult;
        private bool isErrorHappens = false;
        private DispatcherTimer timer = new DispatcherTimer();

        public NumberPage() {
            InitializeComponent();
            InitTimer();

            bgWorker.DoWork += NumberPageBgWorker_DoWork;
            bgWorker.RunWorkerCompleted += NumberPageBgWorker_RunWorkerCompleted;
        }

        private void NumberPageBgWorker_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN NumberPageBgWorker_DoWork()");
                timer.Stop();
                string password = pswBox.Password;
                autorizeResult = new AutorizeResult();
                
                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("NumberPageBgWorker_DoWork() Task Run()...");
                    autorizeResult = await ClientEngine.AuthorizeAndGetTicketAsync(password);

                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT NumberPageBgWorker_DoWork; resetEvent.Wait() ended");

            } catch (Exception ex) {
                isErrorHappens = true;
                logger.Error(ex, "NumberPageBgWorker_DoWork() EXCEPTION");
            }
        }

        private void NumberPageBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug("IN NumberPageBgWorker_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (isErrorHappens) {
                isErrorHappens = false;
                logger.Error($"NumberPageBgWorker_RunWorkerCompleted() something wrong");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;

            } else {
                pswBox.Clear();
                logger.Debug($"NumberPageBgWorker_RunWorkerCompleted() autorizeResult.Result={autorizeResult.Result}");

                switch (autorizeResult.Result) {
                    case AutorizeResultType.Authorized:
                        logger.Debug("NumberPageBgWorker_RunWorkerCompleted() - Navigate to TestPage...");
                        timer.Stop();
                        NavigationService.Navigate(new TestPage(autorizeResult.Ticket));
                        break;
                    case AutorizeResultType.Unauthorized:
                        modalMessage.Text = "Пароль неверный";
                        modal.IsOpen = true;
                        ResetTimer();
                        break;
                    default:
                        modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                        modal.IsOpen = true;
                        ResetTimer();
                        break;
                }
            }

            logger.Debug("OUT NumberPageBgWorker_RunWorkerCompleted()");
        }

        private void btnNumber_Click(object sender, RoutedEventArgs e) {

            string buttonValue = ((Button)sender).Tag as string;
            if (!bgWorker.IsBusy && pswBox.Password.Length < 10) {
                pswBox.Password += buttonValue;
            }
            ResetTimer();
        }

        private void btnResetClick(object sender, RoutedEventArgs e) {
            logger.Debug("btnResetClick()");
            pswBox.Clear();
            ResetTimer();
        }

        private void btnApplyPassword_Click(object sender, RoutedEventArgs e) {
            logger.Debug("IN btnApplyPassword_Click()");
            if (!bgWorker.IsBusy && pswBox.Password.Length >= 4) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorker.RunWorkerAsync()...");
                bgWorker.RunWorkerAsync();
            }
            ResetTimer();

            logger.Debug("OUT btnApplyPassword_Click()");
        }

        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
            ResetTimer();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            timer.Stop();
            NavigationService.Navigate(new StartPage());
        }

        private void InitTimer() {
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = TimeSpan.FromSeconds(AppSettingsHelper.Settings.ScreensaverTimerIntervalSeconds);
            timer.Start();
        }

        private void ResetTimer() {
            timer.Stop();
            timer.Start();
        }

    }
}

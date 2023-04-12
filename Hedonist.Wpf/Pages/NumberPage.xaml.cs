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

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for Number.xaml
    /// </summary>
    public partial class NumberPage : Page {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly BackgroundWorker bgWorker = new BackgroundWorker();
       //private readonly ManualResetEventSlim resetEvent = new();
        private AutorizeResult autorizeResult;
        private bool isErrorHappens = false;

        public NumberPage() {
            InitializeComponent();

            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN BgWorker_DoWork()");
                string password = pswBox.Password;
                autorizeResult = new AutorizeResult();
                
                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorker_DoWork() Task Run()...");
                    autorizeResult = await ClientEngine.AuthorizeAndGetTicketAsync(password);
                    //resetEvent.Set();
                    logger.Debug("BgWorker_DoWork() Task Run() resetEvent was Set()");

                });
                Task.WaitAll(getAuthTask);
                logger.Debug("BgWorker_DoWork; resetEvent.Wait() started...");
                //resetEvent.Wait();
                logger.Debug("OUT BgWorker_DoWork; resetEvent.Wait() ended");

            } catch (Exception ex) {
                isErrorHappens = true;
                logger.Error("OUT BgWorker_DoWork() with EXCEPTION", ex);
            }
        }

        private void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug("IN BgWorker_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (isErrorHappens) {
                isErrorHappens = false;
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;

            } else {
                pswBox.Clear();

                switch (autorizeResult.Result) {
                    case AutorizeResultType.Authorized:
                        logger.Debug("IN BgWorker_RunWorkerCompleted() - Navigate to TestPage...");
                        NavigationService.Navigate(new TestPage(autorizeResult.Ticket));
                        break;
                    case AutorizeResultType.Unauthorized:
                        modalMessage.Text = "Неверный пароль";
                        modal.IsOpen = true;
                        break;
                    default:
                        modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                        modal.IsOpen = true;
                        break;
                }
            }
            logger.Debug("OUT BgWorker_RunWorkerCompleted()");
        }

        private void btnNumber_Click(object sender, RoutedEventArgs e) {

            string buttonValue = ((Button)sender).Tag as string;
            if (!bgWorker.IsBusy && pswBox.Password.Length < 10) {
                pswBox.Password += buttonValue;
            }
        }

        private void btnApplyPassword_Click(object sender, RoutedEventArgs e) {
            logger.Debug("IN btnApplyPassword_Click()");
            if (!bgWorker.IsBusy && pswBox.Password.Length >= 4) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorker.RunWorkerAsync()...");
                bgWorker.RunWorkerAsync();
            }
            logger.Debug("OUT btnApplyPassword_Click()");
        }

        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }

    }
}

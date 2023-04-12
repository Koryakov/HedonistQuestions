using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using System.Windows.Shell;

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for Number.xaml
    /// </summary>
    public partial class NumberPage : Page {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly BackgroundWorker bgWorker = new BackgroundWorker();
        private string Token { get; set; } = string.Empty;

        public NumberPage() {
            InitializeComponent();

            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e) {
            string password = pswBox.Password;
            Task.Run(async () => {
                var result = await ClientEngine.AuthorizeAndGetTicketAsync(password); 
            });
            Token = "toekkkkkkkk";
        }

        private void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            NavigationService.Navigate(new TestPage(Token));
        }

        private void btnNumber_Click(object sender, RoutedEventArgs e) {

            string buttonValue = ((Button)sender).Tag as string;
            if (pswBox.Password.Length < 10) {
                pswBox.Password += buttonValue;
            }
        }

        private void btnApplyPassword_Click(object sender, RoutedEventArgs e) {
            if (pswBox.Password.Length >= 4) {
                if (!bgWorker.IsBusy) {
                    bgWorker.RunWorkerAsync();
                }
            }
        }

    }
}

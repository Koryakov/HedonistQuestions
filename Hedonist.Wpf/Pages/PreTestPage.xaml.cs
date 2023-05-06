using System;
using System.Collections.Generic;
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

namespace Hedonist.Wpf.Pages {

    public partial class PreTestPage : Page {
        private string ticket;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public PreTestPage(string ticket) {
            this.ticket = ticket;
            InitializeComponent();
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }

        private void btnStartTest_Click(object sender, RoutedEventArgs e) {
            logger.Debug($"btnStartTest_Click() - Navigate to TestPage, ticket={ticket}");
            NavigationService.Navigate(new TestPage(ticket));
        }
    }
}

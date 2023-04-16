using Hedonist.Wpf.Helpers;
using Microsoft.Extensions.Configuration;
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
using System.Windows.Threading;

namespace Hedonist.Wpf.Pages
{
    public partial class InfoPage : Page {
        
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private DispatcherTimer timer = new DispatcherTimer();
        public InfoPage() {
            InitializeComponent();
            InitTimer();
        }

        private void GridClick(object sender, MouseButtonEventArgs e) {
           ResetTimer();
        }

        private void timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            NavigationService.Navigate(new StartPage());
        }

        private void InitTimer() {
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(AppSettingsHelper.Settings.ScreensaverTimerIntervalSeconds);
            timer.Start();
        }

        private void ResetTimer() {
            timer.Stop();
            timer.Start();
        }
    }
}

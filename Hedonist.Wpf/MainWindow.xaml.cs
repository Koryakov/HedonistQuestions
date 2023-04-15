using Hedonist.Wpf.Pages;
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
using System.Windows.Shapes;

namespace Hedonist.Wpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            frame.NavigationService.Navigate(new StartPage());
            //frame.NavigationService.Navigate(new NumberPage());
            //frame.NavigationService.Navigate(new TestPage("02bfaebf-61cc-42f6-ab38-fbb578809d5c"));
            //frame.NavigationService.Navigate(new Variants());
            //frame.NavigationService.Navigate(new InfoPage());
        }
    }
}

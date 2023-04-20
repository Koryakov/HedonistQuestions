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
    /// <summary>
    /// Interaction logic for HandPage.xaml
    /// </summary>
    public partial class HandPage : Page {
        public HandPage() {
            InitializeComponent();
        }

        private void btnShowResult_Click(object sender, RoutedEventArgs e) {
            //NavigationService.Navigate(new VariantsPage());
        }
    }
}

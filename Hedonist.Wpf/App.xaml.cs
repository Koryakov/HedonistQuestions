using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Hedonist.Wpf {
   
    public partial class App : Application {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            logger.Error(e.Exception, "APPLICATION EXCEPTION");

            e.Handled = true;
        }
    }
}

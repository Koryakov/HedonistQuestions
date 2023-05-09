using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace Hedonist.Wpf {
   
    public partial class App : Application {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            //logger.Error(e.Exception, "APPLICATION EXCEPTION");
            //e.Handled = true;
        }

        public App() : base() {
            SetupUnhandledExceptionHandling();
        }

        private void SetupUnhandledExceptionHandling() {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) => {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached) {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };

            // Catch exceptions from the main UI dispatcher thread.
            // Typically we only need to catch this OR the Dispatcher.UnhandledException.
            // Handling both can result in the exception getting handled twice.
            //Application.Current.DispatcherUnhandledException += (sender, args) =>
            //{
            //	// If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
            //	if (!Debugger.IsAttached)
            //	{
            //		args.Handled = true;
            //		ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
            //	}
            //};
        }

        void ShowUnhandledException(Exception ex, string unhandledExceptionType, bool promptUserForShutdown) {
            logger.Error(ex, "IN APP UNHANDLED EXCEPTION...");

            var title = $"Упс";
            var message = $"Что-то неожиданно пошло не так...Попробовать открыть заново?";

            (Settings settings, string appInfo) = GetApplicationInformationString(logger);

            if (MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }


            logger.Error(ex, $"OUT APP UNHANDLED EXCEPTION...{appInfo}");
            Application.Current.Shutdown();
        }

        public static (Settings settings, string appIinfoString) GetApplicationInformationString(NLog.Logger logger) {
            string appInfo = string.Empty;
            Settings settings = null;
            string version = string.Empty;
            try {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
                version = Assembly.GetEntryAssembly().GetName().Version.ToString();

                settings = config.GetRequiredSection("Settings").Get<Settings>();

                if (settings != null) {
                    appInfo = $"TerminalName='{settings.TerminalName}', TerminalIdentifier='{settings.TerminalIdentifier}'" +
                        $", version='{version}', " +
                        $", QuizHost='{settings.QuizHost}', HttpTimeoutSeconds='{settings.HttpTimeoutSeconds}'" +
                        $", ScreensaverTimerIntervalSeconds='{settings.ScreensaverTimerIntervalSeconds}'" +
                        $", DisplayScale='{settings.DisplayScale}', HideMouseCursor='{settings.HideMouseCursor}'";
                }
            }
            catch (Exception ex2) {
                logger.Error(ex2, "Get app settings failed");
                version = "failed";
            }
            return (settings, appInfo);
        }
    }
}

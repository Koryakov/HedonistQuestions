﻿using Hedonist.Models;
using Hedonist.Wpf.Pages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public MainWindow() {
            (Settings settings, string appInfo) = App.GetApplicationInformationString(logger);

            if (settings.HideMouseCursor) {
                Cursor = Cursors.None;
            }
            logger.Info($"HEDONIST APP STARTED. Information: {appInfo}");
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            //frame.NavigationService.Navigate(new InfoPage());
            frame.NavigationService.Navigate(new StartPage());
            //frame.NavigationService.Navigate(new NumberPage());
            //frame.NavigationService.Navigate(new PreTestPage("752e7875-ba1a-4d5f-b223-93db65994390"));
            //frame.NavigationService.Navigate(new TestPage("752e7875-ba1a-4d5f-b223-93db65994390"));
            //frame.NavigationService.Navigate(new HandPage());
            //frame.NavigationService.Navigate(new GiftMusicPage1());

            //frame.NavigationService.Navigate(new GiftPage(
            //    new GiftPageModel() { 
            //        Ticket = "752e7875-ba1a-4d5f-b223-93db65994390",
            //        SelectedAnswers = new List<Answer> {
            //            new Answer {
            //                Group = 1,
            //                Id = 14,
            //                Order = 1,
            //                ParentAnswerId = null,
            //                QuestionId = 3,
            //                Text = "На вечеринку"
            //            },
            //            new Answer {
            //                Group = 1,
            //                Id = 1,
            //                Order = 1,
            //                ParentAnswerId = 14,
            //                QuestionId = 4,
            //                Text = "На баре любимого клуба узнали"
            //            }
            //        }
            //}));
            //frame.NavigationService.Navigate(new VariantsPage("ca5ea469-82ff-4fb6-961d-9940766929ab"));
        }
    }
}

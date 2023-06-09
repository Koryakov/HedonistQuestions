﻿using Hedonist.Models;
using Hedonist.Wpf.Pages.GiftPages;
using ModalControl;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
using static Hedonist.Wpf.Pages.TestPage;

namespace Hedonist.Wpf.Pages {
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class TestPage : Page {

        public class QuizState {
            //public QuizData Quiz { get; set; } = new ();
            public Queue<Question> QuestionQueue { get; set; } = new ();
            public List<Answer> Answers { get; set; } = new();
            public Queue<Answer> SelectedAnswers { get; set; } = new();
        }

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private BackgroundWorker bgWorkerQuiz = new();
        private BackgroundWorker bgWorkerGift = new();
        private GiftWorker giftWorker;

        private (AutorizeResultType resultType, QuizData quiz) quizDataResponse;
        private bool isErrorHappens = false;
        private QuizState quizState;
        private TestPageModel testPageModel = new();
        private (AutorizeResultType resultType, GiftTypeResult giftTypeResult) giftTypeResponse;

        public TestPage(string ticket) {
            logger.Debug($"IN TestPage() constructor");
            testPageModel.Ticket = ticket;
            InitializeComponent();
            imgHeader.Source = GiftWorker.GetImageSource($"Images", "TestPage-1.jpg");

            btnNext.Visibility = Visibility.Hidden;

            if (quizState == null) {
                StartFirstTime();
            } else {
                BindQuiz();
            }
            logger.Debug("OUT TestPage() constructor");
        }

        public void StartFirstTime() {
            quizState = new QuizState();
            bgWorkerQuiz = new();
            bgWorkerQuiz.DoWork += TestPageBgWorker_DoWork;
            bgWorkerQuiz.RunWorkerCompleted += TestPageBgWorker_RunWorkerCompleted;

            if (!bgWorkerQuiz.IsBusy) {
                spinner.IsLoading = true;
                logger.Debug("starting bgWorkerQuiz.RunWorkerAsync()...");
                bgWorkerQuiz.RunWorkerAsync();
            }
        }

        private void TestPageBgWorker_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN TestPageBgWorker_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("TestPageBgWorker_DoWork() Task Run()...");

                    quizDataResponse = await ClientEngine.GetQuizByTicketAsync(testPageModel.Ticket);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT TestPageBgWorker_DoWork; resetEvent.Wait() ended");

            }
            catch (Exception ex) {
                isErrorHappens = true;
                logger.Error(ex, "TestPageBgWorker_DoWork() EXCEPTION");
            }
        }

        private void TestPageBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug("IN TestPageBgWorker_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (isErrorHappens) {
                logger.Error("TestPageBgWorker_RunWorkerCompleted() isErrorHappens=TRUE");
                isErrorHappens = false;
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            else {
                switch (quizDataResponse.resultType) {
                    case AutorizeResultType.Authorized:
                        quizState.Answers = quizDataResponse.quiz.Answers;
                        foreach (var question in quizDataResponse.quiz.Questions.OrderBy(q => q.Order)) {
                            quizState.QuestionQueue.Enqueue(question);
                        }
                        BindQuiz();
                        break;
                    default:
                        modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                        modal.IsOpen = true;
                        break;
                }
            }
            logger.Debug("OUT TestPageBgWorker_RunWorkerCompleted()");
        }

        private void BindQuiz() {
            logger.Debug("IN BindQuiz()");
            try {
                var itemsData = new List<State>();
                Question currentQuestion = quizState.QuestionQueue.Dequeue();
                IEnumerable<Answer> answersForShowing;
                Answer previousAnswer = quizState.SelectedAnswers.LastOrDefault();

                if (previousAnswer == null) {
                    answersForShowing = quizState.Answers.
                        Where(a => a.QuestionId == currentQuestion.Id && a.ParentAnswerId == null)
                        .OrderBy(a => a.Order).ToList();
                }
                else {
                    answersForShowing = quizState.Answers.Where(a => a.ParentAnswerId == previousAnswer.Id);
                }

                txtQuestion.Text = currentQuestion.Text;
                foreach (var answer in answersForShowing) {
                    itemsData.Add(new State() {
                        StateId = answer.Id,
                        StateName = answer.Text
                    });
                }
                ctlAnswers.ItemsSource = itemsData;
                logger.Debug("OUT BindQuiz()");

            } catch (Exception ex) {
                logger.Error(ex, "BindQuiz() with EXCEPTION");
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e) {
            btnNext.Visibility = Visibility.Hidden;

            State? selectedState = ctlAnswers.ItemsSource.OfType<State>().FirstOrDefault(a => a.IsSelected);
            if(selectedState != null) {
                Answer answer = quizState.Answers.First(a => a.Id == selectedState.StateId);
                if (answer != null) {
                    testPageModel.SelectedAnswers.Add(answer);

                    quizState.SelectedAnswers.Enqueue(answer);

                    if (quizState.QuestionQueue.Count != 0) {
                        imgHeader.Source = GiftWorker.GetImageSource($"Images", "TestPage-2.png");
                        
                        BindQuiz();
                    } else {
                        bgWorkerGift = new ();
                        bgWorkerGift.DoWork += BgWorkerGiftType_DoWork;
                        bgWorkerGift.RunWorkerCompleted += BgWorkerGiftType_RunWorkerCompleted;

                        //Navigate(giftPageModel);
                        if (!bgWorkerGift.IsBusy) {
                            spinner.IsLoading = true;
                            logger.Debug("starting bgWorkerGift.RunWorkerAsync()...");
                            bgWorkerGift.RunWorkerAsync();
                        }
                    }
                }
            }
        }

        private void BgWorkerGiftType_DoWork(object? sender, DoWorkEventArgs e) {
            try {
                logger.Debug("IN BgWorkerGiftType_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorkerGiftType_DoWork() Task Run()...");
                    giftTypeResponse = await ClientEngine.
                        GetGiftTypeByAnswerIdAsync(testPageModel.Ticket, testPageModel.SelectedAnswers.Last().Id);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorkerGiftType_DoWork; resetEvent.Wait() ended");

            }
            catch (TaskCanceledException ex) {
                giftTypeResponse = new(AutorizeResultType.Timeout, null);
                logger.Error(ex, "BgWorkerGiftType_DoWork() TIMEOUT");
            }
            catch (Exception ex) {
                giftTypeResponse = new(AutorizeResultType.Error, null);
                logger.Error(ex, "BgWorkerGiftType_DoWork() EXCEPTION");
            }
        }

        private void BgWorkerGiftType_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
            logger.Debug($"IN BgWorkerGiftType_RunWorkerCompleted()");
            spinner.IsLoading = false;

            if (giftTypeResponse.resultType == AutorizeResultType.Authorized) {
                GiftTypeResult result = giftTypeResponse.giftTypeResult;

                var exData = JObject.Parse(result.GiftType.ExtendedData);
                var giftPage1Data = exData["GiftPage1Data"];

                var model = new GiftsOver.GiftsOverModel() {
                    HeaderText = giftPage1Data["GiftOverText"].ToString(),

                    Ticket = testPageModel.Ticket
                };
                NavigationService.Navigate(new HandPage(testPageModel.Ticket, result.GiftType));
            }
            else {
                modalMessage.Text = "Что-то пошло не так. Попробуйте еще раз";
                modal.IsOpen = true;
            }
            logger.Debug($"OUT BgWorkerGiftType_RunWorkerCompleted()");
        }

        private void OnCloseModalClick(object sender, RoutedEventArgs e) {
            modal.IsOpen = false;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e) {
            btnNext.Visibility = Visibility.Visible;
        }

        private void HomeButtonClick(object sender, MouseButtonEventArgs e) {
            NavigationService.Navigate(new StartPage());
        }
    }
}

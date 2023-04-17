using Hedonist.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Hedonist.Wpf {
    internal enum AutorizeResultType {
        Unknown = 0,
        Error = 1,
        Unauthorized = 2,
        Timeout = 3,
        Authorized = 4
    }
    internal class AutorizeResult {
        public AutorizeResultType Result { get; set; }
        public string Ticket { get; set;}
    }

    internal class Settings {
        public double HttpTimeoutSeconds { get; set; }
        public double ScreensaverTimerIntervalSeconds { get; set; }
        public string TerminalName { get; set; }
        public string TerminalIdentifier { get; set; }
        public string QuizHost { get; set; }
    }

    internal class ClientEngine {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static HttpClient httpClient;
        static Settings settings = new Settings();
        static ClientEngine() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            settings = config.GetRequiredSection("Settings").Get<Settings>();

            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds);

            logger.Info($"new ClientEngine() completed; Settings: QuizHost='{settings.QuizHost}', TerminalName='{settings.TerminalName}';");
        }

        public static async Task<AutorizeResult> AuthorizeAndGetTicketAsync(string password) {
            logger.Debug("IN AuthorizeAndGetTicketAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            try {
                var authData = new AuthenticationData() {
                    Password = password,
                    TerminalName = settings.TerminalName,
                    DeviceIdentifier = settings.TerminalIdentifier
                };
                var json = JsonConvert.SerializeObject(authData);
                var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/Authenticate", requestContent);
                logger.Info($"Authenticate request StatusCode = {response.StatusCode};");

                var authResult = new AutorizeResult();
                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        authResult.Result = AutorizeResultType.Authorized;
                        authResult.Ticket = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        authResult.Result = AutorizeResultType.Unauthorized;
                        break;
                    case HttpStatusCode.InternalServerError:
                        authResult.Result = AutorizeResultType.Error;
                        break;
                    default:
                        authResult.Result = AutorizeResultType.Unknown;
                        break;
                }
                logger.Debug("OUT AuthorizeAndGetTicketAsync();");
                return authResult;

            } catch (HttpRequestException ex) {
                logger.Error(ex, "AuthorizeAndGetTicketAsync GetAsync TIMEOUT;");
                return new AutorizeResult() {
                    Result = AutorizeResultType.Timeout
                };
            }
        }

        public static async Task<(AutorizeResultType resultType, QuizData quizData)> GetQuizByTicketAsync(string ticket) {
            logger.Debug("IN GetQuizByTicketAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            AutorizeResultType resultType = AutorizeResultType.Unknown;
            try {
                response = await httpClient.GetAsync(
                    settings.QuizHost + $"/api/Quizz/GetQuizData?ticket={ticket}");
                logger.Info($"GetQuizByTicketAsync request StatusCode = {response.StatusCode};");

                var strQuiz = await response.Content.ReadAsStringAsync();
                var quizData = JsonConvert.DeserializeObject<QuizData>(strQuiz);

                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        resultType = AutorizeResultType.Authorized;
                        break;
                    case HttpStatusCode.Unauthorized:
                        resultType = AutorizeResultType.Unauthorized;
                        break;
                    case HttpStatusCode.InternalServerError:
                        resultType = AutorizeResultType.Error;
                        break;
                    default:
                        resultType = AutorizeResultType.Unknown;
                        break;
                }
                logger.Debug("OUT GetQuizByTicketAsync();");
                return (resultType, quizData);

            }
            catch (HttpRequestException ex) {
                logger.Error(ex, "GetQuizByTicketAsync GetAsync TIMEOUT;");
                return (AutorizeResultType.Timeout, null);
            }

        }
    }
}
/*
WindowState = "Maximized"
WindowStyle = "None"
*/
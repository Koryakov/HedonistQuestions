using Hedonist.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
        public double HttpTimeoutMilliseconds { get; set; }
        public string TerminalName { get; set; }
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
            httpClient.Timeout = TimeSpan.FromMilliseconds(settings.HttpTimeoutMilliseconds);

            logger.Info($"new ClientEngine() completed; Settings: QuizHost='{settings.QuizHost}', TerminalName='{settings.TerminalName}';");
        }

        public static async Task<AutorizeResult> AuthorizeAndGetTicketAsync(string password) {
            logger.Debug("IN AuthorizeAndGetTicketAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            try {
                response = await httpClient.GetAsync(
                    settings.QuizHost + $"/api/Quizz/Authenticate?password={password}&terminalName={settings.TerminalName}");
                logger.Info($"Authenticate request StatusCode = {response.StatusCode};");

                var authResult = new AutorizeResult();
                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        authResult.Result = AutorizeResultType.Authorized;
                        authResult.Ticket = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.NonAuthoritativeInformation:
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

            } catch (HttpRequestException) {
                logger.Error("AuthorizeAndGetTicketAsync GetAsync TIMEOUT;");
                return new AutorizeResult() {
                    Result = AutorizeResultType.Timeout
                };
            }
        }

        public static async Task<AuthenticatedResult<(List<Question>?, List<Answer>?)>> GetQuizByTicketAsync(string ticket) {
            return null;
        }
    }
}
/*
WindowState = "Maximized"
WindowStyle = "None"
*/
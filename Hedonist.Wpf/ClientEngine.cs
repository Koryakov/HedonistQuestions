﻿using Hedonist.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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

    public class Settings {
        public double HttpTimeoutSeconds { get; set; }
        public double ScreensaverTimerIntervalSeconds { get; set; }
        public double? DisplayScale { get; set; }
        public string TerminalName { get; set; }
        public string TerminalIdentifier { get; set; }
        public string QuizHost { get; set; }
        public bool HideMouseCursor { get; set; }
    }

    internal class ClientEngine {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static HttpClient httpClient;
        static Settings settings = new();
        static ClientEngine() {
            try {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
                settings = config.GetRequiredSection("Settings").Get<Settings>();

                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds);

                logger.Info($"static ClientEngine() completed; Settings: QuizHost='{settings.QuizHost}', TerminalName='{settings.TerminalName}';");
            } catch (Exception ex) {
                logger.Error(ex, "static ClientEngine() Exception;");
            }
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

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/Authenticate?{Guid.NewGuid}", requestContent);
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
                    settings.QuizHost + $"/api/Quizz/GetQuizData?ticket={ticket}&{Guid.NewGuid}");
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

        public static async Task<(AutorizeResultType resultType, Store store)> GetStoreAsync(string ticket, string terminalDeviceId) {
            logger.Debug("IN GetStoreAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            AutorizeResultType resultType = AutorizeResultType.Unknown;
            try {
                var requestData = new RequestedStoreInfo() {
                    Ticket = ticket,
                    TerminalDeviceId = terminalDeviceId
                };

                var json = JsonConvert.SerializeObject(requestData);
                var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/GetStore?{Guid.NewGuid}", requestContent);

                var str = await response.Content.ReadAsStringAsync();
                var store = JsonConvert.DeserializeObject<Store>(str);

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
                logger.Debug("OUT GetStoreAsync();");
                return (resultType, store);

            }
            catch (HttpRequestException ex) {
                logger.Error(ex, "GetStoreAsync GetAsync TIMEOUT;");
                return (AutorizeResultType.Timeout, null);
            }

        }

        public static async Task<(AutorizeResultType resultType, GiftTypeResult giftCommonData)> GetGiftTypeByAnswerIdAsync(string ticket, int answerId) {
            logger.Debug("IN GetGiftTypeByAnswerIdAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            AutorizeResultType resultType = AutorizeResultType.Unknown;
            try {
                var requestedGiftInfo = new RequestedGiftInfo() {
                    Ticket = new Ticket(ticket),
                    SelectedAnswerId = answerId
                };
             
                var json = JsonConvert.SerializeObject(requestedGiftInfo);
                var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/GetGiftTypeByAnswerId?{Guid.NewGuid}", requestContent);

                logger.Info($"GetGiftAsync request StatusCode = {response.StatusCode};");

                var strData = await response.Content.ReadAsStringAsync();
                var qrCodeData = JsonConvert.DeserializeObject<GiftTypeResult>(strData);

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
                logger.Debug("OUT GetGiftTypeByAnswerIdAsync();");
                return (resultType, qrCodeData);

            }
            catch (HttpRequestException ex) {
                logger.Error(ex, "GetGiftTypeByAnswerIdAsync TIMEOUT;");
                return (AutorizeResultType.Timeout, null);
            }
        }

        public static async Task<(AutorizeResultType resultType, GiftCommonData giftCommonData)> GetGiftByTypeAsync(string ticket, int giftTypeId) {
            logger.Debug("IN GetGiftByTypeAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            AutorizeResultType resultType = AutorizeResultType.Unknown;
            try {
                var requestedGiftTypeInfo = new RequestedGiftTypeInfo() {
                    Ticket = ticket,
                    GiftTypeId = giftTypeId
                };

                var json = JsonConvert.SerializeObject(requestedGiftTypeInfo);
                var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/GetGiftByType?{Guid.NewGuid}", requestContent);

                logger.Info($"GetGiftByTypeAsync request StatusCode = {response.StatusCode};");

                var strData = await response.Content.ReadAsStringAsync();
                var giftData = JsonConvert.DeserializeObject<GiftCommonData>(strData);

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
                logger.Debug("OUT GetGiftByTypeAsync();");
                return (resultType, giftData);

            }
            catch (HttpRequestException ex) {
                logger.Error(ex, "GetGiftByTypeAsync TIMEOUT;");
                return (AutorizeResultType.Timeout, null);
            }
        }

        public static async Task<(AutorizeResultType resultType, GiftCommonData qrCodeData)> GetGiftTypeAsync(string ticket, int giftTypeId) {
            logger.Debug("IN GetGiftTypeAsync();");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            AutorizeResultType resultType = AutorizeResultType.Unknown;
            try {
                var parameters = new RequestedGiftTypeInfo {
                    Ticket = ticket,
                    GiftTypeId = giftTypeId
                };

                var json = JsonConvert.SerializeObject(parameters);
                var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/GetGiftType?{Guid.NewGuid}", requestContent);

                logger.Info($"GetGiftTypeAsync request StatusCode = {response.StatusCode};");

                var strData = await response.Content.ReadAsStringAsync();
                var qrCodeData = JsonConvert.DeserializeObject<GiftCommonData>(strData);

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
                logger.Debug("OUT GetGiftTypeAsync();");
                return (resultType, qrCodeData);

            }
            catch (HttpRequestException ex) {
                logger.Error(ex, "GetGiftTypeAsync TIMEOUT;");
                return (AutorizeResultType.Timeout, null);
            }

        }

        //public static async Task<AutorizeResult> GetGiftAsync(GiftPageModel giftPageModel) {
        //    logger.Debug("IN GetGiftAsync();");
        //    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        //    try {
        //        var authData = new AuthenticationData() {
        //            Password = password,
        //            TerminalName = settings.TerminalName,
        //            DeviceIdentifier = settings.TerminalIdentifier
        //        };
        //        var json = JsonConvert.SerializeObject(authData);
        //        var requestContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

        //        response = await httpClient.PostAsync(settings.QuizHost + $"/api/Quizz/Authenticate", requestContent);
        //        //logger.Info($"Authenticate request StatusCode = {response.StatusCode};");

        //        var authResult = new AutorizeResult();
        //        switch (response.StatusCode) {
        //            case HttpStatusCode.OK:
        //                authResult.Result = AutorizeResultType.Authorized;
        //                authResult.Ticket = await response.Content.ReadAsStringAsync();
        //                break;
        //            case HttpStatusCode.Unauthorized:
        //                authResult.Result = AutorizeResultType.Unauthorized;
        //                break;
        //            case HttpStatusCode.InternalServerError:
        //                authResult.Result = AutorizeResultType.Error;
        //                break;
        //            default:
        //                authResult.Result = AutorizeResultType.Unknown;
        //                break;
        //        }
        //        logger.Debug("OUT GetGiftAsync();");
        //        return authResult;

        //    }
        //    catch (HttpRequestException ex) {
        //        logger.Error(ex, "GetGiftAsync GetAsync TIMEOUT;");
        //        return new AutorizeResult() {
        //            Result = AutorizeResultType.Timeout
        //        };
        //    }
        //}
    }
}
/*
WindowState = "Maximized"
WindowStyle = "None"
*/
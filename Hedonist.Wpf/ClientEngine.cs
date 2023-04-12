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
using System.Threading.Tasks;
using System.Windows;

namespace Hedonist.Wpf {

    internal class Settings {
        public double HttpTimeoutMilliseconds { get; set; }
    }

    internal class ClientEngine {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static HttpClient httpClient;
        static Settings settings;
        static ClientEngine() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            settings = config.GetRequiredSection("Settings").Get<Settings>();
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(settings.HttpTimeoutMilliseconds);
        }

        public static async Task<(HttpStatusCode, string)> AuthorizeAndGetTicketAsync(string password) {
            logger.Info("IN Authorize;");
            string ticket = string.Empty;
            HttpResponseMessage response = await httpClient.GetAsync($"https://localhost:7130/api/Quizz/Authenticate?password={password}");
            logger.Info($"Authenticate request StatusCode = {response.StatusCode};");

            if (response.IsSuccessStatusCode) {
                ticket = await response.Content.ReadAsStringAsync();
            }
            return (response.StatusCode, ticket);
        }
    }
}
/*
WindowState = "Maximized"
WindowStyle = "None"
*/
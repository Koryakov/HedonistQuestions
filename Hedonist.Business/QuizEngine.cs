using Hedonist.Models;
using Hedonist.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Business {
    public class QuizEngine {

        HedonistRepository repository;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public QuizEngine() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            string connectionString = config.GetConnectionString("HedonistConnectionString");
            repository = new HedonistRepository(connectionString);
        }

        public async Task<QuestionsModel> GetQuizAsync(string password) {
            try {
                QuestionsModel questInfo = new QuestionsModel();

                string hashPsw = PasswordHasher.Hash(password);
                (bool, string) res = await repository.ValidatePasswordHashAsync(hashPsw);

                questInfo.IsPasswordValid = res.Item1;
                if (questInfo.IsPasswordValid) {
                    questInfo.Ticket = res.Item2;
                    questInfo.Questions = await repository.GetQuizByTicketAsync(res.Item2);
                }
                return questInfo;
            }
            catch (Exception ex) {
                logger.Error(ex);
                throw;
            }
        }
    }
}

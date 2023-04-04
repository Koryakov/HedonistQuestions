using Hedonist.Repositories;
using Hedonist.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Repository {
    public class HedonistRepository {
        private readonly string connectionString;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public HedonistRepository(string connectionString) {
            this.connectionString = connectionString;
        }

        protected HedonistDbContext CreateContext() {
            return new HedonistDbContext(connectionString);
        }
        /// <returns>returns ticket for next requests</returns>
        public async Task<(bool, string)> ValidatePasswordHashAsync(string passwordHash) {
            using (var db = CreateContext()) {
                var pswInfo = await db.PasswordInfo.FirstOrDefaultAsync(p => p.PasswordHash == passwordHash);
                if (pswInfo != null) {
                    logger.Info($"passwordHash={passwordHash} found. IsUsed={pswInfo.IsUsed}. Ticket='{pswInfo.Ticket}'");

                    if (!pswInfo.IsUsed) {
                        //TODO: generate ticket, return and store to db
                        return (true, pswInfo.Ticket);
                    }
                }

                logger.Info($"passwordHash={passwordHash} not found.");
                return (false, "");
            }
        }

        public async Task<List<Question>> GetQuizByTicketAsync(string ticket) {
            using (var db = CreateContext()) {
                var questions = await db.Question.Where(q => q.Group == 1).ToListAsync();
                return questions;
            }
        }
    }
}

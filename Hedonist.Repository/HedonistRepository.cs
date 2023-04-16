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
using System.Runtime.Intrinsics.Arm;

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
        public async Task<AuthenticatedResult<string>> UsePasswordAndReturnTicketAsync(string passwordHash, string psw, string terminalName) {
            using (var db = CreateContext()) {

                string ticket = Guid.NewGuid().ToString();
                var pswInfo = await db.PasswordInfo.FirstOrDefaultAsync(p => p.PasswordHash == passwordHash);
                bool isSuccess = (pswInfo != null);

                logger.Info($"UsePasswordAndReturnTicketAsync() isSuccess={isSuccess},  passwordHash={passwordHash}. Ticket='{ticket}'");

                LoginAttempt la = new LoginAttempt() {
                    Psw = psw,
                    CreatedDate = DateTime.UtcNow,
                    TerminalName = terminalName,
                    Ticket = ticket,
                    IsSuccess = isSuccess
                };

                await db.LoginAttempt.AddAsync(la);
                await db.SaveChangesAsync();

                if (pswInfo != null) {
                    return new AuthenticatedResult<string>() {
                        IsAuthorized = true,
                        Result = ticket
                    };
                }
                else {
                    return AuthenticatedResult<string>.NotAuthenticated();
                }
            }
        }

        public async Task<AuthenticatedResult<(List<Question>? questions, List<Answer>? answers)>> GetQuizByTicketAsync(string ticket) {
            const int group = 1;
            using (var db = CreateContext()) {
                bool isTicketCorrect = await db.LoginAttempt.AnyAsync(p => p.Ticket == ticket);

                if (isTicketCorrect) {
                    var questions = await db.Question.Where(q => q.Group == group).Include(q => q.Answers).ToListAsync();
                    var answers = await db.Answer.Where(a => a.Group == group).ToListAsync();

                    return new AuthenticatedResult<(List<Question>?, List<Answer>?)>() {
                        IsAuthorized = true,
                        Result = (questions, answers)
                    };
                }
                return AuthenticatedResult<(List<Question>?, List<Answer>?)>.NotAuthenticated();
            }
        }

        public async Task<AuthenticatedResult<(List<Question>?, List<Answer>?)>> GetQuizByTicketAsyncOld(string ticket) {
            const int group = 1;
            using (var db = CreateContext()) {
                bool isTicketCorrect = await db.PasswordInfo.AnyAsync(p => p.Ticket == ticket);

                if (isTicketCorrect) {
                    var questions = await db.Question.Where(q => q.Group == group).ToListAsync();
                    var answers = await db.Answer.Where(a => a.Group == group).ToListAsync();

                    return new AuthenticatedResult<(List<Question>?, List<Answer>?)>() {
                        IsAuthorized = true,
                        Result = (questions, answers)
                    };
                }
                return AuthenticatedResult<(List<Question>?, List<Answer>?)>.NotAuthenticated();
            }
        }
    }
}

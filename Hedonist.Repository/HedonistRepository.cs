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
        public async Task<AuthenticatedResult<string>> UsePasswordAndReturnTicketAsync(string passwordHash, string terminalName) {
            using (var db = CreateContext()) {
                var pswInfo = await db.PasswordInfo.FirstOrDefaultAsync(p => p.PasswordHash == passwordHash);
                if (pswInfo != null) {
                    logger.Info($"passwordHash={passwordHash} found. IsUsed={pswInfo.IsUsed}. Ticket='{pswInfo.Ticket}'");

                    if (!pswInfo.IsUsed) {
                        pswInfo.IsUsed = true;
                        pswInfo.Ticket = Guid.NewGuid().ToString();
                        pswInfo.TerminalName = terminalName;
                        await db.SaveChangesAsync();

                        return new AuthenticatedResult<string>() {
                            IsAuthorized = true,
                            Result = pswInfo.Ticket
                        };
                    }                    
                }
                logger.Info($"passwordHash={passwordHash} not found.");
                return AuthenticatedResult<string>.NotAuthenticated();
            }
        }

        public async Task<AuthenticatedResult<(List<Question>? questions, List<Answer>? answers)>> GetQuizByTicketAsync(string ticket) {
            const int group = 1;
            using (var db = CreateContext()) {
                bool isTicketCorrect = await db.PasswordInfo.AnyAsync(p => p.Ticket == ticket);

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

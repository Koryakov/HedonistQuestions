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
using System.Data;

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
        public async Task<AuthenticatedResult<string>> UsePasswordAndReturnTicketAsync(string passwordHash, AuthenticationData authData) {
            using (var db = CreateContext()) {

                string ticket = Guid.NewGuid().ToString();
                var pswInfo = await db.PasswordInfo.FirstOrDefaultAsync(p => p.PasswordHash == passwordHash);
                var terminal = await db.Terminal.FirstOrDefaultAsync(t => t.DeviceIdentifier == authData.DeviceIdentifier);
                bool isSuccess = (pswInfo != null);

                logger.Info($"UsePasswordAndReturnTicketAsync() isSuccess={isSuccess},  passwordHash={passwordHash}. Ticket='{ticket}'");

                LoginAttempt la = new LoginAttempt() {
                    Psw = authData.Password,
                    CreatedDate = DateTime.UtcNow,
                    SentTerminalName = authData.TerminalName,
                    SentDeviceIdentifier = authData.DeviceIdentifier,
                    Ticket = ticket,
                    IsSuccess = isSuccess,
                    IsExpired = false
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
                bool isTicketCorrect = await db.LoginAttempt.AnyAsync(p => p.Ticket == ticket && p.IsExpired == false);

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

        public async Task<AuthenticatedResult<GiftFromDbResult>> GetGiftAsync(RequestedGiftInfo soldGiftData) {
            logger.Info($"IN GetGiftAsync(), ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
            var result = new GiftFromDbResult();

            using (var db = CreateContext()) {

                LoginAttempt? loginAttempt = await db.LoginAttempt
                    .FirstOrDefaultAsync(l => l.Ticket == soldGiftData.Ticket.Value && l.IsExpired == false);

                if (loginAttempt == null) {
                    return AuthenticatedResult<GiftFromDbResult>.NotAuthenticated();
                }
                else {

                    Terminal? terminal = await db.Terminal.FirstOrDefaultAsync(t => t.DeviceIdentifier == loginAttempt.SentDeviceIdentifier);

                    if (terminal != null) {
                        Answer? answer = await db.Answer
                            .Include(answer => answer.GiftTypes)
                            .ThenInclude(giftType => giftType.Stores.Where(s => s.Id == terminal.StoreId))
                            .FirstOrDefaultAsync(a => a.Id == soldGiftData.SelectedAnswerId);

                        if (answer != null) {
                            foreach (var giftType in answer.GiftTypes) {

                                if (giftType.Stores.Any(s => s.Id == terminal.StoreId)) {
                                    var gift = await db.Gift
                                        //.Include(g => g.GiftType)
                                        .FirstOrDefaultAsync(g => !g.IsSold && g.GiftTypeId == giftType.Id);

                                    if (gift != null) {
                                        gift.IsSold = true;
                                        await db.SaveChangesAsync();

                                        logger.Info($"GetGiftAsync() found, sold giftId={gift.Id}");
                                        result = new GiftFromDbResult() {
                                            Gift = gift,
                                            GiftType = giftType,
                                            GetGiftResultType = GetGiftResultType.GiftFound
                                        };
                                    }
                                }
                            }
                            if (result.GetGiftResultType != GetGiftResultType.GiftFound) {
                                result.GetGiftResultType = GetGiftResultType.NoFreeGift;
                                logger.Error($"GetGiftAsync(), FREE GIFT NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                            }
                        }
                        else {
                            result.GetGiftResultType = GetGiftResultType.AnswerNotFound;
                            logger.Error($"GetGiftAsync(), ANSWER NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                        }
                    }
                    else {
                        result.GetGiftResultType = GetGiftResultType.TerminalNotFound;
                        logger.Error($"GetGiftAsync(), TERMINAL NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                    }
                }
                logger.Info($"OUT GetGiftAsync()");
                return new AuthenticatedResult<GiftFromDbResult>(result);
            }
        }
    }
}

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

        public async Task<AuthenticatedResult<Store>> GetStoreAsync(RequestedStoreInfo info) {
            
            using (var db = CreateContext()) {
                bool isTicketCorrect = await db.LoginAttempt.AnyAsync(p => p.Ticket == info.Ticket && p.IsExpired == false);

                if (isTicketCorrect) {
                    Terminal? terminal = await db.Terminal.FirstOrDefaultAsync(t => t.DeviceIdentifier == info.TerminalDeviceId);

                    Store? store = null;
                    if (terminal != null) {
                        store = db.Store.Include(s => s.GiftTypes).FirstOrDefault(s => s.Id == terminal.StoreId);
                    }
                    return new AuthenticatedResult<Store>() {
                        IsAuthorized = true,
                        Result = store
                    };
                }
                return AuthenticatedResult<Store>.NotAuthenticated();
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

        public async Task<AuthenticatedResult<GiftTypeResult>> GetGiftTypeByAnswerIdAsync(RequestedGiftInfo soldGiftData) {

            logger.Info($"IN GetGiftTypeByAnswerIdAsync(), ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
            var result = new GiftTypeResult();

            using (var db = CreateContext()) {

                LoginAttempt? loginAttempt = await db.LoginAttempt
                    .FirstOrDefaultAsync(l => l.Ticket == soldGiftData.Ticket.Value && l.IsExpired == false);

                if (loginAttempt == null) {
                    return AuthenticatedResult<GiftTypeResult>.NotAuthenticated();
                }
                else {

                    Terminal? terminal = await db.Terminal.FirstOrDefaultAsync(t => t.DeviceIdentifier == loginAttempt.SentDeviceIdentifier);

                    if (terminal != null) {
                        Answer? answer = await db.Answer
                            .Include(answer => answer.GiftTypes)
                            .ThenInclude(giftType => giftType.Stores.Where(s => s.Id == terminal.StoreId))
                            .FirstOrDefaultAsync(a => a.Id == soldGiftData.SelectedAnswerId);

                        if (answer != null) {
                            result.GiftType = answer.GiftTypes.Where(t => t.Stores.Any(s => s.Id == terminal.StoreId)).FirstOrDefault();
                            
                            if (result.GiftType == null) {
                                result.ResultType = GiftTypeResultType.StoreHasNoGiftType;
                                result.GiftType = answer.GiftTypes.FirstOrDefault();

                                logger.Error($"GetGiftTypeByAnswerIdAsync(), STORE HAS NO GIFTTYPE. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                            } else {
                                result.ResultType = GiftTypeResultType.Success;
                            }
                        }
                        else {
                            result.ResultType = GiftTypeResultType.AnswerNotFound;
                            logger.Error($"GetGiftTypeByAnswerIdAsync(), ANSWER NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                        }
                    }
                    else {
                        result.ResultType = GiftTypeResultType.TerminalNotFound;
                        logger.Error($"GetGiftTypeByAnswerIdAsync(), TERMINAL NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={soldGiftData.Ticket.Value}, answerId={soldGiftData.SelectedAnswerId}");
                    }
                }
                logger.Info($"OUT GetGiftTypeByAnswerIdAsync()");
                return new AuthenticatedResult<GiftTypeResult>(result);
            }
        }

        public async Task<AuthenticatedResult<GiftFromDbResult>> GetGiftByTypeAsync(RequestedGiftTypeInfo info) {

            logger.Info($"IN GetGiftByTypeAsync(), ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
            var result = new GiftFromDbResult();

            using (var db = CreateContext()) {

                LoginAttempt? loginAttempt = await db.LoginAttempt
                    .FirstOrDefaultAsync(l => l.Ticket == info.Ticket && l.IsExpired == false);

                if (loginAttempt == null) {
                    return AuthenticatedResult<GiftFromDbResult>.NotAuthenticated();
                }
                else {
                    var gift = await db.Gift.Include(g => g.GiftType).FirstOrDefaultAsync(g => !g.IsSold && g.GiftTypeId == info.GiftTypeId);
                    if (gift != null) {
                        gift.IsSold = true;
                        await db.SaveChangesAsync();

                        result.GetGiftResultType = GetGiftResultType.GiftFound;
                        result.Gift = gift;
                        result.GiftType = gift.GiftType;
                        logger.Info($"GetGiftByTypeAsync() found, sold giftId={gift.Id}, DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                    }
                    else {
                        result.GiftType = await db.GiftType.FirstOrDefaultAsync(g => g.Id == info.GiftTypeId);
                        result.GetGiftResultType = GetGiftResultType.NoFreeGift;
                        logger.Error($"GetGiftByTypeAsync(), FREE GIFT NOT FOUND. DeviceIdentifier={loginAttempt.SentDeviceIdentifier}. ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                    }
                }
                logger.Info($"OUT GetGiftByTypeAsync()");
                return new AuthenticatedResult<GiftFromDbResult>(result);
            }
        }

        public async Task<AuthenticatedResult<GiftTypeResult>> GetGiftTypeByIdAsync(RequestedGiftTypeInfo info) {
            logger.Info($"IN GetGiftTypeByIdAsync(), ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
            var result = new GiftTypeResult();

            using (var db = CreateContext()) {

                LoginAttempt? loginAttempt = await db.LoginAttempt
                    .FirstOrDefaultAsync(l => l.Ticket == info.Ticket && l.IsExpired == false);

                if (loginAttempt == null) {
                    return AuthenticatedResult<GiftTypeResult>.NotAuthenticated();
                }
                else {
                    var giftType = db.GiftType.Where(gt => gt.Id == info.GiftTypeId).FirstOrDefault();
                    if (giftType == null) {
                        logger.Error($"OUT GetGiftTypeByIdAsync() GiftType not found. giftTypeId={info.GiftTypeId}");
                        return new AuthenticatedResult<GiftTypeResult>(
                            new GiftTypeResult() {
                                GiftType = null
                            });
                    }
                    else {
                        logger.Info($"OUT GetGiftTypeByIdAsync()");

                        return new AuthenticatedResult<GiftTypeResult>(
                            new GiftTypeResult() {
                                GiftType = giftType
                            });
                    }
                }
            }
        }
    }
}

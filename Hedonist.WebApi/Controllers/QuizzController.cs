using Hedonist.Business;
using Hedonist.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hedonist.WebApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzController : ControllerBase {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public QuizzController() {
            logger.Info("constructor");
        }

        [HttpPost]
        [Route("Authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Authenticate(AuthenticationData authData) {
            try {
                logger.Info($"IN Authenticate(password={authData.Password}, terminal name = {authData.TerminalName}, device id= {authData.DeviceIdentifier})");
                var engine = new QuizEngine();
                var res = await engine.UsePasswordAndReturnTicketAsync(authData);
                if (res.IsAuthorized) {
                    return res.Result;
                }
                else {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex) {
                logger.Error(ex, "Authenticate() with EXCEPTION");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("GetGift")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftCommonData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GiftCommonData>> GetGift(RequestedGiftInfo requestedGiftInfo) {
            try {
                logger.Info($"IN GetGift(), ticket={requestedGiftInfo.Ticket.Value}, answerId={requestedGiftInfo.SelectedAnswerId}");
                var giftResult = await new QuizEngine().GetGiftAsync(requestedGiftInfo);

                if (!giftResult.IsAuthorized) {
                    logger.Debug($"OUT GetQuizData(ticket={requestedGiftInfo.Ticket.Value}), return Status401Unauthorized");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (giftResult.Result == null) {
                    logger.Debug($"OUT GetGift() return Status404NotFound, ticket={requestedGiftInfo.Ticket.Value}, answerId={requestedGiftInfo.SelectedAnswerId}");
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                logger.Debug($"OUT GetGift() return OK, CertificateCode={giftResult.Result.CertificateCode}, ticket={requestedGiftInfo.Ticket.Value}, answerId={requestedGiftInfo.SelectedAnswerId}");

                return Ok(giftResult.Result);
            }
            catch (Exception ex) {
                logger.Error(ex, $"GetGift() EXCEPTION, return Status401Unauthorized, ticket={requestedGiftInfo.Ticket.Value}, answerId={requestedGiftInfo.SelectedAnswerId}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("GetGiftType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftCommonData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GiftCommonData>> GetGiftType(RequestedGiftTypeInfo info) {
            try {
                logger.Info($"IN GetGiftType(), ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                var giftResult = await new QuizEngine().GetGiftTypeByIdAsync(info);

                if (!giftResult.IsAuthorized) {
                    logger.Debug($"OUT GetQuizData(ticket={info.Ticket}), return Status401Unauthorized");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (giftResult.Result == null) {
                    logger.Debug($"OUT GetGiftType() return Status404NotFound, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                logger.Debug($"OUT GetGiftType() return OK, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");

                return Ok(giftResult.Result);
            }
            catch (Exception ex) {
                logger.Error(ex, $"GetGiftType() EXCEPTION, return Status401Unauthorized, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("GetGiftByType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftCommonData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GiftCommonData>> GetGiftByType(RequestedGiftTypeInfo info) {
            try {
                logger.Info($"IN GetGiftByType(), ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                var giftResult = await new QuizEngine().GetGiftByTypeAsync(info);

                if (!giftResult.IsAuthorized) {
                    logger.Debug($"OUT GetGiftByType(ticket={info.Ticket}), return Status401Unauthorized");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (giftResult.Result == null) {
                    logger.Debug($"OUT GetGiftByType() return Status404NotFound, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                logger.Debug($"OUT GetGiftByType() return OK, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");

                return Ok(giftResult.Result);
            }
            catch (Exception ex) {
                logger.Error(ex, $"GetGiftByType() EXCEPTION, return Status401Unauthorized, ticket={info.Ticket}, giftTypeId={info.GiftTypeId}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("GetStore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Store))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Store>> GetStore(RequestedStoreInfo info) {
            try {
                logger.Info($"IN GetStore(), ticket={info.Ticket}, TerminalDeviceId={info.TerminalDeviceId}");
                var storeResult = await new QuizEngine().GetStoreAsync(info);

                if (!storeResult.IsAuthorized) {
                    logger.Debug($"OUT GetStore(ticket={info.Ticket}), return Status401Unauthorized");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (storeResult.Result == null) {
                    logger.Debug($"OUT GetStore() return Status404NotFound, ticket={info.Ticket}, TerminalDeviceId={info.TerminalDeviceId}");
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                logger.Debug($"OUT GetStore() return OK, ticket={info.Ticket}, TerminalDeviceId={info.TerminalDeviceId}");

                return Ok(storeResult.Result);
            }
            catch (Exception ex) {
                logger.Error(ex, $"GetStore() EXCEPTION, return Status401Unauthorized, ticket={info.Ticket}, TerminalDeviceId={info.TerminalDeviceId}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: api/<QuizzController>
        [HttpGet]
        [Route("GetQuizData")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuizData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<QuizData>> GetQuizData(string ticket) {
            try {
                logger.Debug($"IN GetQuizData(ticket={ticket})");
                var quizInfo = await new QuizEngine().GetQuizDataAsync(new Ticket(ticket));
                QuizData quizData = quizInfo.Result;

                if (!quizInfo.IsAuthorized) {
                    logger.Debug($"OUT GetQuizData(ticket={ticket}) return Status401Unauthorized");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (quizData.Questions.Count == 0 || quizData.Answers.Count == 0) {
                    logger.Debug($"OUT GetQuizData(ticket={ticket}) return Status404NotFound");
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                logger.Debug($"OUT GetQuizData(ticket={ticket}) return OK, questions count = {quizData.Questions.Count}, answers count = {quizData.Answers.Count}");
                return quizData;
            }
            catch (Exception ex) {
                logger.Error(ex, $"GetQuizData(ticket={ticket}) EXCEPTION");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Question>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GiftQrCodeRawData>> GetGift(RequestedGiftInfo requestedGiftInfo) {
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

        // GET: api/<QuizzController>
        [HttpGet]
        [Route("GetQuizData")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Question>))]
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

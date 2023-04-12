using Hedonist.Business;
using Hedonist.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hedonist.WebApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzController : ControllerBase {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public QuizzController() {
            logger.Info("constructor");
        }

        [HttpGet]
        [Route("Authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Authenticate(string password, string terminalName) {
            try {
                logger.Info($"IN Authenticate(password={password})");
                var engine = new QuizEngine();
                var res = await engine.UsePasswordAndReturnTicketAsync(new PasswordData(password, terminalName));
                if (res.IsAuthorized) {
                    return res.Result;
                }
                else {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex) {
                logger.Error(ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: api/<QuizzController>
        [HttpGet]
        [Route("GetQuiz")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Question>))]
        [ProducesResponseType(StatusCodes.Status203NonAuthoritative)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Question>>> GetQuiz(string ticket) {
            try {
                logger.Info($"IN GetQuestions(ticket={ticket})");
                var engine = new QuizEngine();
                var quizInfo = await engine.GetQuizAsync(new Ticket(ticket));
                List<Question>? questions = quizInfo.Result;

                if (!quizInfo.IsAuthorized) {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (questions.Count == 0 || questions[0].Answers.Count == 0) {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                return questions;
            }
            catch (Exception ex) {
                logger.Error(ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        //// GET api/<QuizzController>/5
        //[HttpGet("{id}")]
        //public string Get(int id) {
        //    return "value";
        //}

        //// POST api/<QuizzController>
        //[HttpPost]
        //public void Post([FromBody] string value) {
        //}

        //// PUT api/<QuizzController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value) {
        //}

        //// DELETE api/<QuizzController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id) {
        //}
    }
}

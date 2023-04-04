using Hedonist.Business;
using Hedonist.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hedonist.WebApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzController : ControllerBase {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public QuizzController() {
            logger.Info("constructor");
        }

        // GET: api/<QuizzController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Question>))]
        [ProducesResponseType(StatusCodes.Status203NonAuthoritative)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<QuestionsModel>> GetQuestionsAsync(string password) {
            try {
                logger.Info($"IN GetQuestions(password={password})");
                var engine = new QuizEngine();
                var questInfo = await engine.GetQuizAsync(password);
                if (!questInfo.IsPasswordValid) {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (questInfo.Questions.Count == 0) {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                return questInfo;
            } catch (Exception ex) { 
                logger.Error(ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        // GET api/<QuizzController>/5
        [HttpGet("{id}")]
        public string Get(int id) {
            return "value";
        }

        // POST api/<QuizzController>
        [HttpPost]
        public void Post([FromBody] string value) {
        }

        // PUT api/<QuizzController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/<QuizzController>/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }
    }
}

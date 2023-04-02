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
        public IEnumerable<string> Get() {
            logger.Info("Get");
            return new string[] { "value1", "value2" };
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

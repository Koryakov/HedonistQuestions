using Hedonist.Business;
using Hedonist.Models;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Hedonist.Test {
    public class UnitTest {
        private IConfiguration config;

        public UnitTest() {
        }

        [Fact]
        public void PasswordsCreatingTest() {
            StringBuilder sqlInstructions = new StringBuilder($"INSERT INTO public.\"password_info\"\r\n(password_hash, is_used)\r\nVALUES\r\n");
            for (int i = 0; i < 100; i++) {
                string psw = $"*{i:000}";
                string hashPsw = PasswordHasher.Hash(psw);
                Assert.True(PasswordHasher.Verify(psw, hashPsw));

                sqlInstructions.AppendLine($"('{hashPsw}', FALSE),");
            }
            var res = sqlInstructions.ToString();
        }

        [Fact]
        public async Task GetQuizTest() {
            var engine = new QuizEngine();
            var result = await engine.UsePasswordAndReturnTicketAsync(new Password("Psw-6"));
            Assert.True(result.IsAuthorized);
            string ticket = result.Result;
            
            var questionsModel = await engine.GetQuizAsync(new Ticket(ticket));
        }
    }
}
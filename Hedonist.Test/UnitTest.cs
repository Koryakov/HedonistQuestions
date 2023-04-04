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
                string psw = $"Psw-{i}";
                string hashPsw = PasswordHasher.Hash(psw);
                Assert.True(PasswordHasher.Verify(psw, hashPsw));

                sqlInstructions.AppendLine($"('{hashPsw}', FALSE),");
            }
            var res = sqlInstructions.ToString();
        }

        [Fact]
        public async Task ValidatePasswordTest() {
            var engine = new QuizEngine();
            var questionsModel = await engine.GetQuizAsync("Psw-1");
            Assert.True(questionsModel.IsPasswordValid);
        }
    }
}
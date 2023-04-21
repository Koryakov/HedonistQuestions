using Hedonist.Business;
using Hedonist.Models;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.IO;
using System.Text;

namespace Hedonist.Test {
    public class UnitTest {
        private IConfiguration config;

        public UnitTest() {
        }

        [Fact]
        public void PasswordsCreatingTest() {
            StringBuilder sqlInstructions = new StringBuilder($"INSERT INTO public.\"password_info\"\r\n(password_hash, is_used, terminal_name)\r\nVALUES\r\n");
            for (int i = 0; i < 100; i++) {
                string psw = $"*{i:000}";
                string hashPsw = PasswordHasher.Hash(psw);
                Assert.True(PasswordHasher.Verify(psw, hashPsw));

                sqlInstructions.AppendLine($"('{hashPsw}', FALSE, 'unittest_terminal'),");
            }
            var res = sqlInstructions.ToString();
        }

        [Fact]
        public async Task GetGiftTest() {
            var engine = new QuizEngine();

            RequestedGiftInfo giftData = new RequestedGiftInfo() {
                //Ticket = new Ticket("46b1e824-cd2d-4973-96b0-b00bd2acc8dc"),//Gypsy еда без кода, НЕ яндекс
                Ticket = new Ticket("1a75bcec-9d50-47a1-a540-c02ac78b8e4c"),//Депо (яндекс еда с куар кодом)

                SelectedAnswerId = 21
            };
            var result = await engine.GetGiftAsync(giftData);

            Assert.True(result.IsAuthorized);

            //if(result.IsAuthorized) {
            //    using (var ms = new MemoryStream(result.Result.QrCodeByteArr)) {
            //        var img = Image.FromStream(ms);
            //    }
            //}
        }

        //[Fact]
        //public async Task GetQuizTest() {
        //    var engine = new QuizEngine();
        //    var result = await engine.UsePasswordAndReturnTicketAsync(new PasswordData("Psw-6", "unit test"));
        //    Assert.True(result.IsAuthorized);
        //    string ticket = result.Result;

        //    var questionsModel = await engine.GetQuizAsync(new Ticket(ticket));
        //}
    }
}
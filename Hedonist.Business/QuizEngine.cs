using Hedonist.Models;
using Hedonist.Repository;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Hedonist.Business {
    public class QuizEngine {

        HedonistRepository repository;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public QuizEngine() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            string connectionString = config.GetConnectionString("HedonistConnectionString");
            repository = new HedonistRepository(connectionString);
        }

        public async Task<AuthenticatedResult<string>> UsePasswordAndReturnTicketAsync(AuthenticationData authData) {

            string hashPsw = PasswordHasher.Hash(authData.Password);
            return  await repository.UsePasswordAndReturnTicketAsync(hashPsw, authData);
        }

        public async Task<AuthenticatedResult<List<Question>?>> GetQuizAsync(Ticket ticket) {
            var quizResult = await repository.GetQuizByTicketAsync(ticket.Value);
            if (quizResult.IsAuthorized) {
                var questions = quizResult.Result.Item1.OrderBy(q => q.Order).ToList();
                var answersGroups = quizResult.Result.Item2.GroupBy(a => a.ParentAnswerId).OrderBy(g => g.Key).ToList();

                var answersParentGroup = answersGroups[0].ToList();
                //foreach (var answParent in answersParentGroup) {
                //    //answParent.ChildAnswers = answersGroups[1].Where(a => a.ParentId == answParent.Id).ToList();
                //}
                questions[0].Answers = answersParentGroup;

                return new AuthenticatedResult<List<Question>?>() {
                    IsAuthorized = quizResult.IsAuthorized,
                    Result = questions
                };
            }
            else {
                return AuthenticatedResult<List<Question>?>.NotAuthenticated();
            }
        }

        public async Task<AuthenticatedResult<QuizData>> GetQuizDataAsync(Ticket ticket) {
            var quizResult = await repository.GetQuizByTicketAsync(ticket.Value);
            if (quizResult.IsAuthorized) {
                var quizData = new QuizData() {
                    Questions = quizResult.Result.questions,
                    Answers = quizResult.Result.answers
                };
                return new AuthenticatedResult<QuizData>(quizData);
            }
            else {
                return AuthenticatedResult<QuizData>.NotAuthenticated();
            }
        }

        public async Task<AuthenticatedResult<HedonistGiftQrCodeData>> GetGiftAsync(RequestedGiftInfo requestedGiftInfo) {

            var giftResult = await repository.GetGiftAsync(requestedGiftInfo);
            if (giftResult.IsAuthorized) {
                string qrCodeText = String.Format($"{giftResult.Result.giftType.DescriptionPattern}", giftResult.Result.gift.CertificateCode);

                var qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

                var qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeByteArr = qrCode.GetGraphic(20);

                return new AuthenticatedResult<HedonistGiftQrCodeData>() {
                    IsAuthorized = true,
                    Result = new HedonistGiftQrCodeData() {
                        CertificateCode = giftResult.Result.gift.CertificateCode,
                        QrCodeByteArr = qrCodeByteArr
                    }
                };
            }
            return AuthenticatedResult<HedonistGiftQrCodeData>.NotAuthenticated();
        }
    }
}

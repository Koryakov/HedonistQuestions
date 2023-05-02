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
            return await repository.UsePasswordAndReturnTicketAsync(hashPsw, authData);
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


        public async Task<AuthenticatedResult<GiftTypeResult>> GetGiftTypeByAnswerIdAsync(RequestedGiftInfo requestedGiftInfo) {
            var result = await repository.GetGiftTypeByAnswerIdAsync(requestedGiftInfo);

            if (!result.IsAuthorized) {
                return AuthenticatedResult<GiftTypeResult>.NotAuthenticated();
            }
            else {
                return new AuthenticatedResult<GiftTypeResult>() {
                    IsAuthorized = true,
                    Result = result.Result
                };
            }
        }

        public async Task<AuthenticatedResult<GiftCommonData>> GetGiftByTypeAsync(RequestedGiftTypeInfo info) {


            var giftResult = await repository.GetGiftByTypeAsync(info);

            if (!giftResult.IsAuthorized) {
                return AuthenticatedResult<GiftCommonData>.NotAuthenticated();
            }
            else {
                GiftFromDbResult giftFromDb = giftResult.Result;
                var qrCodeData = new GiftCommonData();
                qrCodeData.GiftType = giftFromDb.GiftType;

                if (giftFromDb.GetGiftResultType == GetGiftResultType.GiftFound) {
                    string qrCodeText = String.Format($"{giftResult.Result.GiftType.DescriptionPattern}", giftResult.Result.Gift.CertificateCode);

                    if (qrCodeData.GiftType.HasQrCode) {
                        //qrCodeData.QrCodeByteArr = CreateQrCodeByteArray(qrCodeText);
                        qrCodeData.CertificateCode = giftFromDb.Gift.CertificateCode;
                    }

                    qrCodeData.GiftResult = GiftCommonData.GiftResultType.GiftFound;
                    qrCodeData.QrCodeText = qrCodeText;

                }
                else if (giftFromDb.GetGiftResultType == GetGiftResultType.NoFreeGift) {
                    qrCodeData.GiftResult = GiftCommonData.GiftResultType.NoFreeGift;
                }
                else if (giftFromDb.GetGiftResultType == GetGiftResultType.StoreHasNoGiftType) {
                    qrCodeData.GiftResult = GiftCommonData.GiftResultType.StoreHasNoGiftType;
                }
                else {
                    qrCodeData.GiftResult = GiftCommonData.GiftResultType.InconsistentData;
                }

                return new AuthenticatedResult<GiftCommonData>() {
                    IsAuthorized = true,
                    Result = qrCodeData
                };
            }
        }

        public async Task<AuthenticatedResult<Store>> GetStoreAsync(RequestedStoreInfo info) {

            return await repository.GetStoreAsync(info);
        }

        public async Task<AuthenticatedResult<GiftTypeResult>> GetGiftTypeByIdAsync(RequestedGiftTypeInfo info) {

            return await repository.GetGiftTypeByIdAsync(info);
        }

        private static byte[] CreateQrCodeByteArray(string qrCodeText) {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        public async Task<List<GiftsPurchasedRepotrModel>> GetPurchasedReportAsync() {
            var result = await repository.GetPurchasedGiftsAsync();
            var list = new List<GiftsPurchasedRepotrModel>();
            foreach (var purchase in result) {
                list.Add(new() {
                    Id = purchase.Id,
                    Terminal = purchase.LoginAttempt.SentDeviceIdentifier,
                    Password = purchase.LoginAttempt.Psw,
                    Type = purchase.Gift.GiftType.Name,
                    Certificate = purchase.CertificateCode,
                    Date = purchase.CreatedDate
                }
                    );
            }
            return list;
        }

        public async Task<string> GetPurchasedReportSCVStringAsync() {
            var list = await GetPurchasedReportAsync();
            var stringResult = new StringBuilder();
            foreach (var data in list.OrderByDescending(m => m.Date).ToList()) {
                var dt = new DateTime(data.Date.Year, data.Date.Month, data.Date.Day
                    , data.Date.Hour, data.Date.Minute, data.Date.Second, data.Date.Millisecond
                    , DateTimeKind.Unspecified).ToLocalTime();
                stringResult.AppendFormat($"{data.Id},{data.Terminal},{data.Password},{data.Type},'{data.Certificate}',{dt.ToString("yyyy-MM-dd HHHH:mm:ss")}\n");
            }
            return stringResult.ToString();
        }
    }
}

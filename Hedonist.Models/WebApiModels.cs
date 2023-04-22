using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Models {

    public class AuthenticationData {
        public string Password { get; set; }
        public string DeviceIdentifier { get; set; }
        public string TerminalName { get; set; }

    }
    public class QuizData {
        public List<Question> Questions { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class RequestedGiftInfo {
        public Ticket Ticket { get; set; }
        public int SelectedAnswerId { get; set; }
    }

    public class RequestedStoreInfo {
        public string Ticket { get; set; }
        public string TerminalDeviceId { get; set; }
    }

    public class RequestedGiftTypeInfo {
        public string Ticket { get; set; }
        public int GiftTypeId { get; set; }

    }

    public class GiftCommonData {
        public enum GiftResultType {
            Unknown = 0,
            GiftFound = 1,
            NoFreeGift = 2,
            InconsistentData = 3,
        }

        public GiftResultType GiftResult { get; set; }
        public GiftType GiftType { get; set; }
        public string? CertificateCode { get; set; }
        public string QrCodeText { get; set; }
        public byte[] QrCodeByteArr { get; set; }
    }

    public enum GetGiftResultType {
        Unknown = 0,
        GiftFound = 1,
        NoFreeGift = 2,
        AnswerNotFound = 3,
        TerminalNotFound = 4,
        StoreHasNoGiftType = 6
    }

    public class GiftFromDbResult {
        public GetGiftResultType GetGiftResultType { get; set; }
        public Gift? Gift { get; set; }
        public GiftType GiftType { get; set; }

    }

    public class GiftTypeResult {
        public GiftType? GiftType { get; set; }

    }
}

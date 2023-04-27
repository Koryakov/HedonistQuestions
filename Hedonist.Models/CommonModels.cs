using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Hedonist.Models {
    public class Terminal {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DeviceIdentifier { get; set; }
        public int StoreId { get; set; }
    }

    [Table("question")]
    public class Question {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("group")]
        public int Group { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("text")]
        public string Text { get; set; }

        //[ForeignKey("QuestionId")]
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
    public class Answer {

        [Key]
        public int Id { get; set; }
        public int? ParentAnswerId { get; set; }
        public int Group { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public List<GiftType> GiftTypes { get; set; } = new();
    }


    public class Gift {
        [Key]
        public int Id { get; set; }

        public string CertificateCode { get; set; }
        public int GiftTypeId { get; set; }
        public bool IsSold { get; set; }
        public DateTime CreatedDate { get; set; }
        public GiftType GiftType { get; set; } = new();
    }

    public class GiftType {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string DescriptionPattern { get; set; }
        public string? ExtendedData { get; set; }

        public string Key { get; set; }
        public bool HasQrCode { get; set; }

        public List<Answer> Answers { get; set; }
        public List<Store> Stores { get; set; }
        //public List<GiftGroup> GiftGroups { get; set; }
    }

    public class Store {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GiftType> GiftTypes { get; set; }
        //public List<GiftGroup> GiftGroups { get; set; }
    }

    public class GiftTypeStore {
        public int StoresId { get; set; }
        public int GiftTypesId { get; set; }
        public int GiftGroupsId { get; set; }
    }

    public class GiftGroup {
        [Key]
        public int Id { get; set; }
        public int GiftsCount { get; set; }
        public string Comment { get; set; }
        public List<Store> Stores { get; set; }
        //public List<GiftType> GiftTypes { get; set; }
    }


    public class GiftPurchase {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GiftId { get; set; }

        [Required]
        public int LoginAttemptId { get; set; }
        public string CertificateCode { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("GiftId")]
        public Gift Gift { get; set; }

        [ForeignKey("LoginAttemptId")]
        public LoginAttempt LoginAttempt { get; set; }
    }

    //[Table("gift_history")]
    //public class GiftHistory {

    //    [Key]
    //    [Column("id")]
    //    public int Id { get; set; }

    //    [Column("sertificate_code")]
    //    public string SertificateCode { get; set; }

    //    [Column("remaining_count")]
    //    public int RemainingCount { get; set; }

    //    [Column("created_date")]
    //    public DateTime CreatedDate { get; set; }
    //}

    [Table("password_info")]
    public class PasswordInfo {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }

        [Column("ticket")]
        public string? Ticket { get; set; }

        [Column("terminal_name")]
        public string TerminalName { get; set; }

    }

    public class LoginAttempt {
        [Key]
        public int Id { get; set; }
        public string Psw { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Ticket { get; set; }
        public string SentTerminalName { get; set; }
        public string SentDeviceIdentifier { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsExpired { get; set; }
    }

    public class AuthenticatedResult<T> {
        public static AuthenticatedResult<T> NotAuthenticated() {
            return new AuthenticatedResult<T>() {
                IsAuthorized = false,
                Result = default(T)
            };
        }
        public AuthenticatedResult(T result) {
            IsAuthorized = true;
            Result = result;
        }
        public AuthenticatedResult() { }
        public bool IsAuthorized { get; set; }
        public T? Result { get; set; }
    }

    public class PasswordData {
        public PasswordData(string value, string terminalName) {
            PasswordText = value;
            TerminalName = terminalName;
        }

        public string PasswordText { get; set; }
        public string TerminalName { get; set; }
    }

    public class Ticket {
        public Ticket(string value) {
            Value = value;
        }
        public string Value { get; set; }
    }

}
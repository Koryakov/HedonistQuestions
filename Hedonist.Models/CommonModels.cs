using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hedonist.Models {
    [Table("terminal")]
    public class Terminal {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("mac_address")]
        public string MacAddress { get; set; }
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

    [Table("answer")]
    public class Answer {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("parent_answer_id")]
        public int? ParentAnswerId { get; set; }
        [Column("group")]
        public int Group { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("text")]
        public string Text { get; set; }
        
        [Column("question_id")]
        //[ForeignKey("question_id")]
        public int QuestionId { get; set; }

        //[NotMapped]        public List<Answer> ChildAnswers { get; set; } = new List<Answer>();

    }

    [Table("gift")]
    public class Gift {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sertificate_code")]
        public string SertificateCode { get; set; }
        [Column("remaining_count")]
        public int RemainingCount { get; set; }
    }

    [Table("gift_history")]
    public class GiftHistory {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sertificate_code")]
        public string SertificateCode { get; set; }
        
        [Column("remaining_count")]
        public int RemainingCount { get; set; }
        
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
    }

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

    [Table("login_attempt")]
    public class LoginAttempt {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("psw")]
        public string Psw { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("ticket")]
        public string? Ticket { get; set; }

        [Column("terminal_name")]
        public string TerminalName { get; set; }

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
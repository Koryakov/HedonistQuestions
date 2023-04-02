using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hedonist.Models {
    [Table("terminal")]
    public class Terminal {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

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

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

    }

    [Table("answer")]
    public class Answer {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("parent_id")]
        public int ParentId { get; set; }
        [Column("group")]
        public int Group { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

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

    [Table("password")]
    public class Password {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("hash")]
        public string Hash { get; set; }
        
        //TODO: CREATE DB TRIGGERS!!!

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
        
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServiceNet8.Entities.SYS
{
    [Table("SYS_UserPasswordReset")]
    public class SYS_UserPasswordReset
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        public string ResetToken { get; set; } = string.Empty;

        public DateTime? TokenExpires { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        [ForeignKey("UserID")]
        public SYS_User? User { get; set; }
    }
}

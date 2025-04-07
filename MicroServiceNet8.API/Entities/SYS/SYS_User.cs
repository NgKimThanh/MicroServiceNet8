using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServiceNet8.Entities.SYS
{
    [Table("SYS_User")]
    public class SYS_User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public virtual ICollection<SYS_UserRefreshToken> UserRefreshToken { get; set; }
    }
}

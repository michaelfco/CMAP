using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.Domain.Entities.OB
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Guid UseCredentialId { get; set; }

        // Navigation properties
        //public virtual ICollection<CompanyEndUser> CompanyEndUsers { get; set; } = new List<CompanyEndUser>();
        //public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
        //public virtual ICollection<GoCardlessSetting> GoCardlessSettings { get; set; } = new List<GoCardlessSetting>();
        //public virtual ICollection<RepositoryStorage> RepositoryStorages { get; set; } = new List<RepositoryStorage>();
    }
}

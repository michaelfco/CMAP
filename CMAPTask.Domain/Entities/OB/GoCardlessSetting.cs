using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Domain.Entities.OB
{
    public class GoCardlessSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ConfigId { get; set; }
       
        public Guid? UserId { get; set; }       

        [Required]
        [StringLength(500)]
        public string SecretID { get; set; }

        [Required]
        [StringLength(1000)]
        public string SecretKey { get; set; }

        [StringLength(1000)]
        public string? AccessToken { get; set; }

        [StringLength(1000)]
        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }

       
        [StringLength(50)]
        public string? Environment { get; set; }

        [StringLength(500)]
        public string BaseUrl { get; set; }

        [StringLength(500)]
        public string? WebhookSecret { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

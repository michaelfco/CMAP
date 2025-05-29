using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Domain.Entities.OB
{
    public class Credit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CreditId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int TotalCredits { get; set; } = 0;

        [Required]
        public int CreditsUsed { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

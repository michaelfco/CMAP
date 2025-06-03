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
    public class CreditUsage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CreditUsageId { get; set; }

        [Required]
        public Guid UserId { get; set; }       
        public Guid EndUserId { get; set; }

        public Guid? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Status Status { get; set; }
    }
}

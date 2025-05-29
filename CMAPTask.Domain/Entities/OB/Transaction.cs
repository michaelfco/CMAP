using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Domain.Entities.OB
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TransactionId { get; set; }

        [Required]
        public Guid EndUserId { get; set; }

        [ForeignKey("EndUserId")]
        public virtual CompanyEndUser EndUser { get; set; }

        [Required]
        public string JsonData { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

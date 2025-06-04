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
      

       // [ForeignKey("EndUserId")]
        //public virtual CompanyEndUser EndUser { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string JsonData { get; set; }

        public DateTime? PeriodStart { get; set; } = null;

        public DateTime? PeriodEnd { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Currency { get; set; }
        public Guid AgreementId { get; set; }
        public Guid ConsentId { get; set; }
        public Guid Reference { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

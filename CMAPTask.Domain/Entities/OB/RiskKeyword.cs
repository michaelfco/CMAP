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
    public class RiskKeyword
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeywordId { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Keyword { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(10)]
        public string? Region { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Status Status { get; set; }

        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? UserId { get; set; }
    }
}

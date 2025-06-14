﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenBanking.Domain.Enums.Enum;
using System.Runtime.Serialization;

namespace OpenBanking.Domain.Entities.OB
{
    public class CompanyEndUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid EndUserId { get; set; }

        [Required]
        public Guid UserId { get; set; }        

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? ExternalReference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Status Status { get; set; }
   
        public int? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

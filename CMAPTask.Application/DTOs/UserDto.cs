using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.Application.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UseCredentialId { get; set; }
        public string Environment { get; set; }
        public Guid? GoCardlessConfigId { get; set; }
    }
}

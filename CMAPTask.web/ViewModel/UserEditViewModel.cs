using Microsoft.AspNetCore.Mvc.Rendering;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.web.ViewModel
{
    public class UserEditViewModel
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UseCredentialId { get; set; }
        public List<SelectListItem> GoCardlessSettingsOptions { get; set; } = new List<SelectListItem>();
    }
}

using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.web.ViewModel
{
    public class UserViewModel
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

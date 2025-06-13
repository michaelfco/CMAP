using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenBanking.Domain.Entities.OB;

namespace OpenBanking.web.ViewModel
{
    public class UserProfileViewModel
    {
        [BindNever]
        public User? User { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; }
    }
}

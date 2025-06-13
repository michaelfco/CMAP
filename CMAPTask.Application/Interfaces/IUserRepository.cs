using OpenBanking.Application.Common.Models;
using OpenBanking.Application.DTOs;
using OpenBanking.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> SaveAsync(User user);
        Task<IEnumerable<UserDto>> GetAllAsync(Guid userId);
        Task<User> GetByUserIdAsync(Guid userId);
        Task<User> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task<IEnumerable<GoCardlessSetting>> GetGoCardlessSettingsAsync();
    }
}

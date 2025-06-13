using OpenBanking.Application.Common.Models;
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

        Task<IEnumerable<User>> GetAllAsync(Guid userId);
        Task<User> GetByUserIdAsync(Guid userId);
        Task<User> GetByEmailAsync(string email);
        Task<Guid> SaveAsync(User user);
        Task UpdateAsync(User user);
    }
}

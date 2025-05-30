using OpenBanking.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.Application.Interfaces
{
    public interface ICompanyEndUserRepository
    {
        Task<Guid> SaveAsync(CompanyEndUser user);
        Task<IEnumerable<CompanyEndUser>> GetAllAsync(Guid userId, Status? status);
    }
}

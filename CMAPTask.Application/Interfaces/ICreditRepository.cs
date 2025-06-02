using OpenBanking.Application.DTOs;
using OpenBanking.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.Interfaces
{
    public interface ICreditRepository
    {
        Task<CreditDto> GetCreditUsage(Guid userId);
        Task<Guid> SaveAsync(Credit credit);
        Task<Guid> AddPendingCreditUsageAsync(CreditUsage credit);
        Task<bool> UpdateCreditUsage(Guid endUserId, Guid transactionId);
    }
}

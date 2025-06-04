using OpenBanking.Application.DTOs;
using OpenBanking.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.Interfaces
{
    public interface IApiSettingsRepository
    {
        Task<ApiSettingsDto> GetByEnvironment(Guid? configId);
    }
}

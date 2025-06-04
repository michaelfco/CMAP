using Dapper;
using OpenBanking.Application.DTOs;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.Infrastructure.Repository
{
   
    public class ApiSettingsRepository : IApiSettingsRepository
    {
        private readonly IDapperGenericRepository _repo;
        public ApiSettingsRepository(IDapperGenericRepository repo)
        {
            _repo = repo;
        }

    
        public async Task<ApiSettingsDto> GetByEnvironment(Guid? configId)
        {
            var sql = @"	SELECT TOP 1
                                ConfigId,
	                            BaseUrl, 
	                            SecretID,
	                            SecretKey,
	                            Environment
                            FROM
	                            [dbo].[GoCardlessSettings]
                            WHERE
	                            ConfigId = @configId
                            ";

            var parameters = new DynamicParameters();
            parameters.Add("configId", configId);
          

            return await _repo.QueryFirstOrDefaultAsync<ApiSettingsDto>(sql, parameters);
        }
    }
}

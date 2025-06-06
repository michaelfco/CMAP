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
   
    public class CreditRepository : ICreditRepository
    {
        private readonly IDapperGenericRepository _repo;
        public CreditRepository(IDapperGenericRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> SaveAsync(Credit credit)
        {
            credit.CreditId = Guid.NewGuid();
            credit.CreatedAt = DateTime.UtcNow;

            await _repo.InsertAsync(credit, "Credits");

            return credit.CreditId;
        }


        public async Task<Guid> AddPendingCreditUsageAsync(CreditUsage credit)
        {
            credit.CreditUsageId = Guid.NewGuid();
            credit.CreatedAt = DateTime.UtcNow;           
            

            await _repo.InsertAsync(credit, "CreditUsages");

            return credit.CreditUsageId;
        }

        public async Task<bool> UpdateCreditUsage(Guid endUserId, Guid transactionId)
        {
            var usage = await _repo.GetByIdAsync<CreditUsage>("CreditUsages", "EndUserId", endUserId);

            usage.Status = Status.Complete;
            usage.TransactionId = transactionId;

            await _repo.UpdateAsync(usage, "CreditUsages", "EndUserId");
            return true;
        }

        public async Task<CreditDto> GetCreditUsage(Guid userId)
        {
            var sql = @"	SELECT
                            c.UserId,
                            SUM(c.TotalCredits) AS Quantity,
                            ISNULL(u.PendingCredit, 0) AS PendingCredit,
                            ISNULL(u.UsedCredit, 0) AS UsedCredit,
                            SUM(c.TotalCredits) - ISNULL(u.PendingCredit, 0) - ISNULL(u.UsedCredit, 0) AS ActiveCredit
                        FROM
                            Credits c
                        LEFT JOIN (
                            SELECT
                                UserId,
                                COUNT(CASE WHEN Status = 1 THEN 1 END) AS PendingCredit,
                                COUNT(CASE WHEN Status = 2 THEN 1 END) AS UsedCredit
                            FROM
                                CreditUsages
                            WHERE
                                IsDeleted IS NULL
                            GROUP BY
                                UserId
                        ) u ON c.UserId = u.UserId
                        WHERE
                            c.IsDeleted IS NULL
                            AND c.UserId = @userId
                        GROUP BY
                            c.UserId, u.PendingCredit, u.UsedCredit;
                            ";

            var parameters = new DynamicParameters();
            parameters.Add("userId", userId);
          

            return await _repo.QueryFirstOrDefaultAsync<CreditDto>(sql, parameters);
        }
    }
}

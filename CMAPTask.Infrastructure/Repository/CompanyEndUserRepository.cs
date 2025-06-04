using Dapper;
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
   
    public class CompanyEndUserRepository : ICompanyEndUserRepository
    {
        private readonly IDapperGenericRepository _repo;
        public CompanyEndUserRepository(IDapperGenericRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> SaveAsync(CompanyEndUser user)
        {
            user.EndUserId = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;

            await _repo.InsertAsync(user, "CompanyEndUsers");

            return user.EndUserId;
        }

        public async Task<IEnumerable<CompanyEndUser>> GetAllAsync(Guid userId, Status? status)
        {
            var sql = new StringBuilder("SELECT * FROM CompanyEndUsers WHERE UserId = @userId AND (IsDeleted IS NULL OR IsDeleted = 0)");

            if (status.HasValue)
            {
                sql.Append(" AND Status = @status");
            }

            var parameters = new DynamicParameters();
            parameters.Add("userId", userId);
            if (status.HasValue)
            {
                parameters.Add("status", status);
            }

            var result = await _repo.QueryAsync<CompanyEndUser>(sql.ToString(), parameters);
            return result.ToList();
        }

        public async Task<CompanyEndUser> GetByEndUserIsync(Guid endUserId)
        {
            var sql = @"SELECT * FROM CompanyEndUsers 
                       WHERE EndUserId = @endUserId                       
                       AND (IsDeleted IS NULL OR IsDeleted = 0)";

            var parameters = new DynamicParameters();
            parameters.Add("endUserId", endUserId);          

            return await _repo.QueryFirstOrDefaultAsync<CompanyEndUser>(sql, parameters);
        }

        public async Task<CompanyEndUser> GetByEndUserIdAndPendingAsync(Guid endUserId)
        {
            var sql = @"SELECT * FROM CompanyEndUsers 
                       WHERE EndUserId = @endUserId 
                       AND Status = @status 
                       AND (IsDeleted IS NULL OR IsDeleted = 0)";

            var parameters = new DynamicParameters();
            parameters.Add("endUserId", endUserId);
            parameters.Add("status", Status.pending);

            return await _repo.QueryFirstOrDefaultAsync<CompanyEndUser>(sql, parameters);
        }
    }
}

using Dapper;
using OpenBanking.Application.Common.Models;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDapperGenericRepository _repo;

        public UserRepository(IDapperGenericRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> SaveAsync(User user)
        {
            await _repo.InsertAsync(user, "Users");
            return user.UserId;
        }

        public async Task<IEnumerable<User>> GetAllAsync(Guid userId)
        {
            var sql = "SELECT * FROM Users WHERE (IsDeleted IS NULL OR IsDeleted = 0)";
            var parameters = new DynamicParameters();
            parameters.Add("userId", userId);
            return await _repo.QueryAsync<User>(sql, parameters);
        }

        public async Task<User> GetByUserIdAsync(Guid userId)
        {
            return await _repo.GetByIdAsync<User>("Users", "UserId", userId);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email AND (IsDeleted IS NULL OR IsDeleted = 0)";
            var parameters = new DynamicParameters();
            parameters.Add("Email", email);
            return await _repo.QueryFirstOrDefaultAsync<User>(sql, parameters);
        }

        public async Task UpdateAsync(User user)
        {
            await _repo.UpdateAsync(user, "Users", "UserId");
        }
    }
}

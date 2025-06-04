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
   
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly IDapperGenericRepository _repo;
        public TransactionsRepository(IDapperGenericRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> SaveAsync(Transaction transaction)
        {
            transaction.TransactionId = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;
          

            await _repo.InsertAsync(transaction, "Transactions");

            return transaction.TransactionId;
        }
        public async Task<bool> UpdateStatusRequest(Guid endUserId)
        {
            var transaction = await _repo.GetByIdAsync<CompanyEndUser>("CompanyEndUsers","EndUserId", endUserId);

            transaction.Status = Status.Complete;
            transaction.EndUserId = endUserId;

            await _repo.UpdateAsync(transaction, "CompanyEndUsers", "EndUserId");
            return true;
        }
        public async Task<Transaction> GetCompleteTransactionAsync(Guid endUserId, Guid userId)
        {
            var sql = @"SELECT * FROM Transactions 
                WHERE EndUserId = @endUserId 
                AND (IsDeleted IS NULL OR IsDeleted = 0)";

            var parameters = new DynamicParameters();
            parameters.Add("endUserId", endUserId);

            if (userId != Guid.Empty)
            {
                sql += " AND UserId = @userId";
                parameters.Add("userId", userId);
            }

            return await _repo.QueryFirstOrDefaultAsync<Transaction>(sql, parameters);
        }

    }
}

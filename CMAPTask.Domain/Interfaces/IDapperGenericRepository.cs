using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Domain.Interfaces
{
    public interface IDapperGenericRepository
    {
        Task<int> InsertAsync<T>(T entity, string tableName);
        Task<int> UpdateAsync<T>(T entity, string tableName, string keyColumn);
        Task<IEnumerable<T>> GetAllAsync<T>(string tableName);
        Task<T?> GetByIdAsync<T>(string tableName, string keyColumn, object id);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    }
}

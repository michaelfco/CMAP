using CMAPTask.Infrastructure.Context;
using Dapper;
using Microsoft.EntityFrameworkCore;
using OpenBanking.Domain.Interfaces;
using System.Data;
using System.Reflection;

public class DapperGenericRepository : IDapperGenericRepository
{
    private readonly IDbConnection _db;

    public DapperGenericRepository(OBDbContext context)
    {
        _db = context.Database.GetDbConnection();
    }


    public async Task<int> InsertAsync<T>(T entity, string tableName)
    {
        var props = typeof(T).GetProperties().Where(p => p.GetValue(entity) != null);
        var columns = string.Join(", ", props.Select(p => p.Name));
        var parameters = string.Join(", ", props.Select(p => "@" + p.Name));

        string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        return await _db.ExecuteAsync(sql, entity);
    }

    public async Task<int> UpdateAsync<T>(T entity, string tableName, string keyColumn)
    {
        var props = typeof(T).GetProperties().Where(p => p.Name != keyColumn);
        var setClause = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));

        var sql = $"UPDATE {tableName} SET {setClause} WHERE {keyColumn} = @{keyColumn}";
        return await _db.ExecuteAsync(sql, entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string tableName)
    {
        return await _db.QueryAsync<T>($"SELECT * FROM {tableName}");
    }

    public async Task<T?> GetByIdAsync<T>(string tableName, string keyColumn, object id)
    {
        string sql = $"SELECT * FROM {tableName} WHERE {keyColumn} = @id";
        return await _db.QueryFirstOrDefaultAsync<T>(sql, new { id });
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        return await _db.QueryAsync<T>(sql, parameters);
    }
}
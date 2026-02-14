using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDbLib.Api.Interfaces
{
    /// <summary>
    /// Represents a database transaction boundary.
    /// 
    /// All operations executed within this scope participate
    /// in the same underlying DB transaction.
    /// </summary>
    public interface IDbTransactionScope : IDisposable
    {
        // RAW execution

        /// <summary>
        /// Executes a non-query command inside the transaction.
        /// </summary>
        Task<int> ExecuteAsync(string sql, object parameters = null);

        /// <summary>
        /// Executes a query and maps results.
        /// </summary>
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes a query returning raw rows.
        /// </summary>
        Task<IReadOnlyList<Dictionary<string, object>>> QueryAsync(string sql, object parameters = null);

        /// <summary>
        /// Executes a query returning a single result.
        /// </summary>
        Task<T?> QuerySingleAsync<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes an INSERT and returns generated ID.
        /// </summary>
        Task<int> InsertAndGetIdAsync(string sql, object parameters = null);

        int InsertAndGetId(string sql, object parameters = null);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        Task RollbackAsync();

        int Execute(string sql, object parameters = null);

        IReadOnlyList<T> Query<T>(string sql, object parameters = null) where T : new();

        IReadOnlyList<Dictionary<string, object>> Query(string sql, object parameters = null);

        T? QuerySingle<T>(string sql, object parameters = null) where T : new();

        void Commit();

        void Rollback();
    }
}

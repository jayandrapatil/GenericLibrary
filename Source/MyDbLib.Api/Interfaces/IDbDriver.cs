using MyDbLib.Api.Interfaces;
using MyDbLib.Api.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MyDbLib.Api
{
    /// <summary>
    /// Represents a database driver capable of executing SQL operations.
    /// 
    /// This abstraction isolates database provider differences
    /// (SQL Server, MySQL, Postgres, etc.) from the application layer.
    /// </summary>
    public interface IDbDriver
    {
        #region ASYNC API (RAW)

        /// <summary>
        /// Executes a non-query SQL command asynchronously.
        /// </summary>
        Task<DbCommandResult> ExecuteAsync(string sql, object parameters = null);

        /// <summary>
        /// Executes a query and maps results to strongly-typed models.
        /// </summary>
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes a query and returns raw dictionary rows.
        /// </summary>
        Task<IReadOnlyList<Dictionary<string, object>>> QueryAsync(string sql, object parameters = null);

        /// <summary>
        /// Executes a query and returns a single mapped result.
        /// </summary>
        Task<T?> QuerySingleAsync<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes an INSERT and returns the generated identity value.
        /// </summary>
        Task<int> InsertAndGetIdAsync(string sql, object parameters = null);

        /// <summary>
        /// Begins a new database transaction asynchronously.
        /// </summary>
        Task<IDbTransactionScope> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        #endregion

        #region SYNC API (RAW)

        /// <summary>
        /// Executes a non-query SQL command.
        /// </summary>
        DbCommandResult Execute(string sql, object parameters = null);

        /// <summary>
        /// Executes a query and maps results to strongly-typed models.
        /// </summary>
        IReadOnlyList<T> Query<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes a query and returns raw dictionary rows.
        /// </summary>
        IReadOnlyList<Dictionary<string, object>> Query(string sql, object parameters = null);

        /// <summary>
        /// Executes a query and returns a single mapped result.
        /// </summary>
        T? QuerySingle<T>(string sql, object parameters = null) where T : new();

        /// <summary>
        /// Executes an INSERT and returns the generated identity value.
        /// </summary>
        int InsertAndGetId(string sql, object parameters = null);

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        IDbTransactionScope BeginTransaction(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        #endregion
    }
}

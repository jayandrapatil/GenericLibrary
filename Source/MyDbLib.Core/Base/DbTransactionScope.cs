using MyDbLib.Api.Interfaces;
using MyDbLib.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace MyDbLib.Core.Base
{
    /// <summary>
    /// Represents a transactional execution scope.
    ///
    /// Ensures:
    /// - All operations share the same connection
    /// - All operations share the same transaction
    /// - Retry is NOT applied inside transactions
    /// - Automatic rollback if Commit is not called
    ///
    /// This protects against:
    /// - Partial commits
    /// - Forgotten rollbacks
    /// - Duplicate execution due to retry
    /// </summary>
    internal sealed class DbTransactionScope : IDbTransactionScope
    {
        /// <summary>
        /// Driver used to execute internal DB operations.
        /// </summary>
        private readonly DbDriverBase _driver;

        /// <summary>
        /// Shared connection for the transaction.
        /// </summary>
        private readonly DbConnection _connection;

        /// <summary>
        /// Active DB transaction.
        /// </summary>
        private readonly DbTransaction _transaction;

        /// <summary>
        /// Tracks whether Commit or Rollback has been called.
        /// </summary>
        private bool _completed;

        public DbTransactionScope(
            DbDriverBase driver,
            DbConnection connection,
            DbTransaction transaction)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        #region RAW EXECUTION
        /// <summary>
        /// Executes non-query within the transaction.
        /// </summary>
        public Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            return _driver.ExecuteInternalAsync(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Sync version of ExecuteAsync.
        /// </summary>
        public int Execute(string sql, object parameters = null)
        {
            return _driver.ExecuteInternal(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Executes query returning strongly typed results.
        /// </summary>
        public Task<IReadOnlyList<T>> QueryAsync<T>(
            string sql, object parameters = null) where T : new()
        {
            return _driver.QueryInternalAsync<T>(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Sync version of QueryAsync.
        /// </summary>
        public IReadOnlyList<T> Query<T>(
            string sql, object parameters = null) where T : new()
        {
            return _driver.QueryInternal<T>(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Executes query returning dictionary rows.
        /// </summary>
        public Task<IReadOnlyList<Dictionary<string, object>>> QueryAsync(
            string sql, object parameters = null)
        {
            return _driver.QueryInternalAsync(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Sync version of dictionary query.
        /// </summary>
        public IReadOnlyList<Dictionary<string, object>> Query(
            string sql, object parameters = null)
        {
            return _driver.QueryInternal(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Executes query returning a single record.
        /// </summary>
        public Task<T?> QuerySingleAsync<T>(
            string sql, object parameters = null) where T : new()
        {
            return _driver.QuerySingleInternalAsync<T>(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Sync version of QuerySingleAsync.
        /// </summary>
        public T? QuerySingle<T>(
            string sql, object parameters = null) where T : new()
        {
            var list = Query<T>(sql, parameters);
            return list.Count == 0 ? default : list[0];
        }

        /// <summary>
        /// Executes INSERT and returns generated identity value.
        /// </summary>
        public Task<int> InsertAndGetIdAsync(string sql, object parameters = null)
        {
            return _driver.InsertAndGetIdInternalAsync(
                sql, parameters, _connection, _transaction);
        }

        /// <summary>
        /// Sync version of InsertAndGetIdAsync.
        /// </summary>
        public int InsertAndGetId(string sql, object parameters = null)
        {
            return _driver.InsertAndGetIdInternal(
                sql, parameters, _connection, _transaction);
        }
        #endregion

        #region TRANSACTION CONTROL
        /// <summary>
        /// Commits the transaction.
        /// No-op if already completed.
        /// </summary>
        public Task CommitAsync()
        {
            if (_completed) return Task.CompletedTask;

            _transaction.Commit();
            _completed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Rolls back the transaction.
        /// No-op if already completed.
        /// </summary>
        public Task RollbackAsync()
        {
            if (_completed) return Task.CompletedTask;

            _transaction.Rollback();
            _completed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sync commit.
        /// </summary>
        public void Commit()
        {
            if (_completed) return;

            _transaction.Commit();
            _completed = true;
        }

        /// <summary>
        /// Sync rollback.
        /// </summary>
        public void Rollback()
        {
            if (_completed) return;

            _transaction.Rollback();
            _completed = true;
        }

        #endregion

        #region DISPOSE SAFETY
        /// <summary>
        /// Ensures rollback if Commit was never called.
        /// Prevents accidental partial writes.
        /// </summary>
        public void Dispose()
        {
            if (!_completed)
            {
                try { _transaction.Rollback(); }
                catch { /* best effort */ }
            }

            _transaction.Dispose();
            _connection.Dispose();
        }
        #endregion
    }
}

using MyDbLib.Api;
using MyDbLib.Api.Exceptions;
using MyDbLib.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MyDbLib.Core.Helpers
{
    /// <summary>
    /// Lightweight CRUD helper built on top of IDbDriver / IDbTransactionScope.
    ///
    /// Responsibilities:
    /// - Builds parameterized SQL
    /// - Supports execution via Driver OR Transaction
    /// - Keeps SQL provider-agnostic
    /// - Avoids ORM dependency
    ///
    /// Uses reflection to map POCO/anonymous objects to SQL parameters.
    /// </summary>
    public static class DbCrudHelper
    {
        #region INSERT
        // INSERT helpers support:
        // - Driver execution (auto retry)
        // - Transaction execution (no retry)
        // ASYNC
        public static async Task InsertAsync(IDbDriver driver, string table, object data)
        {
            var (sql, parameters) = BuildInsert(table, data);
            await driver.ExecuteAsync(sql, parameters);
        }

        public static async Task InsertAsync(IDbTransactionScope tx, string table, object data)
        {
            var (sql, parameters) = BuildInsert(table, data);
            await tx.ExecuteAsync(sql, parameters);
        }

        public static async Task<int> InsertAndGetIdAsync(IDbDriver driver, string table, object data)
        {
            var (sql, parameters) = BuildInsertAndGetId(table, data);
            return await driver.InsertAndGetIdAsync(sql, parameters);
        }

        public static async Task<int> InsertAndGetIdAsync(IDbTransactionScope tx, string table, object data)
        {
            var (sql, parameters) = BuildInsertAndGetId(table, data);
            return await tx.InsertAndGetIdAsync(sql, parameters);
        }

        // SYNC
        public static void Insert(IDbDriver driver, string table, object data)
        {
            var (sql, parameters) = BuildInsert(table, data);
            driver.Execute(sql, parameters);
        }

        public static void Insert(IDbTransactionScope tx, string table, object data)
        {
            var (sql, parameters) = BuildInsert(table, data);
            tx.Execute(sql, parameters);
        }

        public static int InsertAndGetId(IDbDriver driver, string table, object data)
        {
            var (sql, parameters) = BuildInsertAndGetId(table, data);
            return driver.InsertAndGetId(sql, parameters);
        }

        public static int InsertAndGetId(IDbTransactionScope tx, string table, object data)
        {
            var (sql, parameters) = BuildInsertAndGetId(table, data);
            return tx.InsertAndGetId(sql, parameters);
        }
        #endregion

        #region UPDATE
        // UPDATE returns number of affected records.
        // ASYNC
        public static async Task<int> UpdateAsync(IDbDriver driver, string table, object data, object where)
        {
            var (sql, parameters) = BuildUpdate(table, data, where);
            var result = await driver.ExecuteAsync(sql, parameters);
            return result.AffectedRecords;
        }

        public static async Task<int> UpdateAsync(IDbTransactionScope tx, string table, object data, object where)
        {
            var (sql, parameters) = BuildUpdate(table, data, where);
            return await tx.ExecuteAsync(sql, parameters);
        }

        // SYNC
        public static int Update(IDbDriver driver, string table, object data, object where)
        {
            var (sql, parameters) = BuildUpdate(table, data, where);
            var result = driver.Execute(sql, parameters);
            return result.AffectedRecords;
        }

        public static int Update(IDbTransactionScope tx, string table, object data, object where)
        {
            var (sql, parameters) = BuildUpdate(table, data, where);
            return tx.Execute(sql, parameters);
        }
        #endregion

        #region DELETE
        // DELETE returns number of affected records.
        // ASYNC
        public static async Task<int> DeleteAsync(IDbDriver driver, string table, object where)
        {
            var (sql, parameters) = BuildDelete(table, where);
            var result = await driver.ExecuteAsync(sql, parameters);
            return result.AffectedRecords;
        }

        public static async Task<int> DeleteAsync(IDbTransactionScope tx, string table, object where)
        {
            var (sql, parameters) = BuildDelete(table, where);
            return await tx.ExecuteAsync(sql, parameters);
        }

        // SYNC
        public static int Delete(IDbDriver driver, string table, object where)
        {
            var (sql, parameters) = BuildDelete(table, where);
            var result = driver.Execute(sql, parameters);
            return result.AffectedRecords;
        }

        public static int Delete(IDbTransactionScope tx, string table, object where)
        {
            var (sql, parameters) = BuildDelete(table, where);
            return tx.Execute(sql, parameters);
        }
        #endregion

        #region SQL BUILDERS
        // SQL builders generate provider-agnostic parameterized statements.
        /// <summary>
        /// Builds INSERT SQL from POCO.
        /// </summary>
        public static (string sql, object parameters) BuildInsert(string table, object data)
        {
            ValidateTable(table);
            var props = GetProps(data);

            var cols = string.Join(", ", props.Select(p => p.Name));
            var vals = string.Join(", ", props.Select(p => "@" + p.Name));

            return ($"INSERT INTO {table} ({cols}) VALUES ({vals});", data);
        }

        /// <summary>
        /// Builds INSERT SQL with identity retrieval placeholder.
        /// </summary>
        public static (string sql, object parameters) BuildInsertAndGetId(string table, object data)
        {
            ValidateTable(table);
            var props = GetProps(data);

            var cols = string.Join(", ", props.Select(p => p.Name));
            var vals = string.Join(", ", props.Select(p => "@" + p.Name));

            var sql = $@"
                INSERT INTO {table} ({cols})
                VALUES ({vals});
                {{IDENTITY}}";

            return (sql, data);
        }

        /// <summary>
        /// Builds UPDATE SQL using data + where objects.
        /// </summary>
        public static (string sql, object parameters) BuildUpdate(string table, object data, object where)
        {
            ValidateTable(table);

            var setProps = GetProps(data);
            var whereProps = GetProps(where);

            var setClause = string.Join(", ", setProps.Select(p => $"{p.Name} = @{p.Name}"));
            var whereClause = string.Join(" AND ", whereProps.Select(p => $"{p.Name} = @w_{p.Name}"));

            var sql = $"UPDATE {table} SET {setClause} WHERE {whereClause};";

            var dict = new Dictionary<string, object>();
            foreach (var p in setProps) dict[p.Name] = p.GetValue(data);
            foreach (var p in whereProps) dict["w_" + p.Name] = p.GetValue(where);

            return (sql, dict);
        }

        /// <summary>
        /// Builds DELETE SQL using where object.
        /// </summary>
        public static (string sql, object parameters) BuildDelete(string table, object where)
        {
            ValidateTable(table);
            var whereProps = GetProps(where);

            var whereClause = string.Join(" AND ", whereProps.Select(p => $"{p.Name} = @{p.Name}"));
            var sql = $"DELETE FROM {table} WHERE {whereClause};";

            return (sql, where);
        }
        #endregion

        #region UTIL
        // Reflection helpers used by SQL builders.
        /// <summary>
        /// Extracts readable properties from an object.
        /// </summary>
        private static PropertyInfo[] GetProps(object obj)
        {
            if (obj == null)
                throw new DbLibException("Data/Where object cannot be null.");

            var props = obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToArray();

            if (props.Length == 0)
                throw new DbLibException("Object has no readable properties.");

            return props;
        }

        /// <summary>
        /// Validates table name is provided.
        /// </summary>
        private static void ValidateTable(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new DbLibException("Table name cannot be empty.");
        }
        #endregion
    }
}

using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Base;
using MySql.Data.MySqlClient;
using System;
using System.Data.Common;

namespace MyDbLib.Providers.MySql
{
    /// <summary>
    /// MySQL implementation of <see cref="DbDriverBase"/>.
    ///
    /// Responsibilities:
    /// - Creates MySQL connections
    /// - Provides MySQL-specific identity retrieval
    ///
    /// All execution logic is handled by the base class.
    /// </summary>
    public sealed class MySqlDriver : DbDriverBase
    {
        #if DEBUG
        public static int InstanceCount = 0;
        #endif

        /// <summary>
        /// MySQL identity retrieval.
        /// </summary>
        protected override string IdentitySelectSql => "SELECT LAST_INSERT_ID();";

        public MySqlDriver(
            string connectionString,
            IRetryPolicy retryPolicy)
            : base(connectionString, retryPolicy)
        {
            #if DEBUG
            InstanceCount++;
            #endif
            //Console.WriteLine("MySQLDriver created");
        }

        /// <summary>
        /// Creates MySQL connection instance.
        /// </summary>
        protected override DbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}

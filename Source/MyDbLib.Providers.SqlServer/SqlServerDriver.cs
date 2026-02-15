using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Base;
using System;
using System.Data.Common;
#if NET8_0
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif


namespace MyDbLib.Providers.SqlServer
{
    /// <summary>
    /// SQL Server implementation of <see cref="DbDriverBase"/>.
    ///
    /// Responsibilities:
    /// - Creates SQL Server connections
    /// - Provides SQL Server-specific identity retrieval
    ///
    /// All execution logic is handled by the base class.
    /// </summary>
    public sealed class SqlServerDriver : DbDriverBase
    {
        #if DEBUG
        public static int InstanceCount = 0;
        #endif

        /// <summary>
        /// SQL Server identity retrieval.
        /// Ensures result is returned as INT.
        /// </summary>
        protected override string IdentitySelectSql => "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public SqlServerDriver(
            string connectionString,
            IRetryPolicy retryPolicy)
            : base(connectionString, retryPolicy)
        {
            #if DEBUG
            InstanceCount++;
            #endif
            //Console.WriteLine("SqlServerDriver created");
        }

        /// <summary>
        /// Creates SQL Server connection instance.
        /// </summary>
        protected override DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}

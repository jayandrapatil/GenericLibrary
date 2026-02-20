using Microsoft.Extensions.DependencyInjection;
using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Factories;
using MyDbLib.Core.Resilience;
using System;

namespace MyDbLib.Providers.SqlServer
{
    /// <summary>
    /// Registers a SQL Server driver with MyDbLib.
    ///
    /// Enables usage of SQL Server under a logical name.
    ///
    /// Example:
    /// services.AddMyDbLibSqlServer("Main", connectionString);
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SQL Server provider to MyDbLib.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="name">Logical database name.</param>
        /// <param name="connectionString">SQL Server connection string.</param>
        public static IServiceCollection AddMyDbLibSqlServer(
            this IServiceCollection services,
            string name,
            string connectionString)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Database name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.", nameof(connectionString));

            // Register metadata for the Core factory
            services.AddSingleton(new DbDriverRegistration(
                name,
                sp =>
                {
                    var retry = sp.GetService<IRetryPolicy>()
                                ?? NoRetryPolicy.Instance;
                    
                    return new SqlServerDriver(connectionString, retry);
                }));

            return services;
        }
    }
}

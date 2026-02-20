using Microsoft.Extensions.DependencyInjection;
using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Factories;
using MyDbLib.Core.Resilience;
using System;

namespace MyDbLib.Providers.MySql
{
    /// <summary>
    /// Registers a MySQL driver with MyDbLib.
    ///
    /// Enables usage of MySQL under a logical name.
    ///
    /// Example:
    /// services.AddMyDbLibMySql("Main", connectionString);
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MySQL provider to MyDbLib.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="name">Logical database name.</param>
        /// <param name="connectionString">MySQL connection string.</param>
        public static IServiceCollection AddMyDbLibMySql(
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
                    // SAFE: Retry is optional
                    var retry = sp.GetService<IRetryPolicy>()
                                ?? NoRetryPolicy.Instance;

                    return new MySqlDriver(connectionString, retry);
                }));

            return services;
        }
    }
}

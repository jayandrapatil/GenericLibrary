using Microsoft.Extensions.DependencyInjection;
using MyDbLib.Api;
using MyDbLib.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyDbLib.Core.Factories
{
    /// <summary>
    /// Resolves database drivers by logical name.
    ///
    /// Enables multi-database scenarios such as:
    /// - Primary DB
    /// - Reporting DB
    /// - Audit DB
    ///
    /// Drivers are registered by providers using
    /// <see cref="DbDriverRegistration"/>.
    ///
    /// Each call to Get() returns a new driver instance.
    /// </summary>
    public sealed class DbDriverFactory : IDbDriverFactory
    {
        /// <summary>
        /// Root service provider used to create driver instances.
        /// </summary>
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Maps logical database name to driver factory function.
        /// </summary>
        private readonly IReadOnlyDictionary<string, Func<IServiceProvider, IDbDriver>> _factories;

        public DbDriverFactory(
            IServiceProvider provider,
            IEnumerable<DbDriverRegistration> registrations)
        {
            _provider = provider;

            _factories = registrations.ToDictionary(
                r => r.Name,
                r => r.Factory,
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a database driver registered under the given name.
        /// </summary>
        /// <param name="name">Logical database name.</param>
        /// <exception cref="DbLibException">
        /// Thrown if the requested database is not registered.
        /// </exception>
        public IDbDriver Get(string name)
        {
            if (!_factories.TryGetValue(name, out var factory))
                throw new DbLibException($"Database '{name}' is not registered.");

            return factory(_provider); // always a new driver
        }
    }
}

using MyDbLib.Api;
using System;

namespace MyDbLib.Core.Factories
{
    /// <summary>
    /// Represents a named database driver registration.
    ///
    /// Maps:
    ///     Logical Name → Driver creation function
    ///
    /// Used internally by <see cref="DbDriverFactory"/>
    /// to resolve database drivers on demand.
    /// </summary>
    public sealed class DbDriverRegistration
    {
        /// <summary>
        /// Logical database name (e.g. "Main", "Reporting").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Factory function used to create a driver instance.
        /// </summary>
        public Func<IServiceProvider, IDbDriver> Factory { get; }

        public DbDriverRegistration(string name, Func<IServiceProvider, IDbDriver> factory)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }
    }
}

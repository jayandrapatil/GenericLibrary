namespace MyDbLib.Api
{
    /// <summary>
    /// Provides access to named database drivers registered in the system.
    /// 
    /// Enables multi-database scenarios such as:
    /// - Primary DB
    /// - Reporting DB
    /// - Audit DB
    /// </summary>
    public interface IDbDriverFactory
    {
        /// <summary>
        /// Returns a database driver registered with the specified logical name.
        /// </summary>
        /// <param name="name">
        /// Logical database name (e.g. "Main", "Reporting").
        /// </param>
        /// <returns>
        /// An <see cref="IDbDriver"/> instance.
        /// </returns>
        IDbDriver Get(string name);
    }
}

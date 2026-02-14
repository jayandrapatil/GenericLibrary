using System;

namespace MyDbLib.Api.Exceptions
{
    /// <summary>
    /// Represents a unified exception type thrown by MyDbLib.
    /// 
    /// This wraps underlying provider exceptions (SQL Server, MySQL, etc.)
    /// to ensure the consuming application does not depend on
    /// database-specific exception types.
    /// </summary>
    public class DbLibException : Exception
    {
        /// <summary>
        /// Creates a new DbLibException with a message.
        /// </summary>
        public DbLibException(string message) : base(message) { }

        /// <summary>
        /// Creates a new DbLibException with a message and inner exception.
        /// </summary>
        public DbLibException(string message, Exception inner)
            : base(message, inner) { }
    }
}

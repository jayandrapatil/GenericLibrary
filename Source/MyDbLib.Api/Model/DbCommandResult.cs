using System;

namespace MyDbLib.Api.Models
{
    /// <summary>
    /// Represents the result of a database command execution.
    /// 
    /// This provides a unified, provider-agnostic way to understand:
    /// - Whether the command succeeded
    /// - How many records were affected
    /// - Error information (if any)
    /// 
    /// This avoids throwing exceptions for expected failures such as:
    /// - Constraint violations
    /// - Deadlocks
    /// - Timeout scenarios (when handled gracefully)
    /// </summary>
    public sealed class DbCommandResult
    {
        /// <summary>
        /// Indicates whether the command executed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of rows affected by the command.
        /// </summary>
        public int AffectedRecords { get; set; }

        /// <summary>
        /// Provider-agnostic error code.
        /// Example: "DEADLOCK", "TIMEOUT", "CONSTRAINT"
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="affected">Number of affected rows.</param>
        public static DbCommandResult Ok(int affected)
        {
            return new DbCommandResult
            {
                Success = true,
                AffectedRecords = affected
            };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="code">Logical error code.</param>
        /// <param name="message">Error message.</param>
        public static DbCommandResult Fail(string code, string message)
        {
            return new DbCommandResult
            {
                Success = false,
                ErrorCode = code,
                ErrorMessage = message
            };
        }
    }
}

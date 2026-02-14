using System;
using System.Threading.Tasks;

namespace MyDbLib.Api.Interfaces
{
    /// <summary>
    /// Defines retry behaviour for transient database failures
    /// such as:
    /// - Deadlocks
    /// - Timeouts
    /// - Network blips
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Executes an async action with retry support.
        /// </summary>
        Task ExecuteAsync(Func<Task> action);

        /// <summary>
        /// Executes an async action returning a value with retry support.
        /// </summary>
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);

        /// <summary>
        /// Executes a synchronous action with retry support.
        /// </summary>
        T Execute<T>(Func<T> action);
    }
}

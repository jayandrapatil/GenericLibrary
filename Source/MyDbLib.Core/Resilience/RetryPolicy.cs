using MyDbLib.Api.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyDbLib.Core.Resilience
{
    /// <summary>
    /// Default implementation of <see cref="IRetryPolicy"/>.
    ///
    /// Provides resilience against transient database failures such as:
    /// - Deadlocks
    /// - Timeouts
    /// - Connection interruptions
    ///
    /// Works in a provider-agnostic manner by analyzing exception types.
    ///
    /// Important:
    /// Retry should NOT be applied inside transactions.
    /// </summary>
    public sealed class RetryPolicy : IRetryPolicy
    {
        /// <summary>
        /// Maximum number of retry attempts.
        /// </summary>
        private readonly int _maxRetries;

        /// <summary>
        /// Delay between retry attempts.
        /// </summary>
        private readonly TimeSpan _delay;

        public RetryPolicy(int maxRetries, TimeSpan delay)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries));

            _maxRetries = maxRetries;
            _delay = delay;
        }

        /// <summary>
        /// Executes an async operation without return value.
        /// </summary>
        public async Task ExecuteAsync(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await ExecuteAsync<object>(async () =>
            {
                await action();
                return null!;
            });
        }

        /// <summary>
        /// Executes an async operation with retry support.
        /// </summary>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            int attempt = 0;

            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsTransient(ex) && attempt < _maxRetries)
                {
                    attempt++;
                    await Task.Delay(_delay);
                }
            }
        }

        /// <summary>
        /// Executes a synchronous operation with retry support.
        /// </summary>
        public T Execute<T>(Func<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            int attempt = 0;

            while (true)
            {
                try
                {
                    return action();
                }
                catch (Exception ex) when (IsTransient(ex) && attempt < _maxRetries)
                {
                    attempt++;
                    Thread.Sleep(_delay);
                }
            }
        }

        /// <summary>
        /// Determines whether an exception represents a transient failure.
        /// Uses provider-agnostic detection.
        /// </summary>
        private static bool IsTransient(Exception ex)
        {
            if (ex == null)
                return false;

            if (ex is TimeoutException)
                return true;

            var name = ex.GetType().Name;

            return name.IndexOf("Deadlock", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Timeout", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Transient", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Connection", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

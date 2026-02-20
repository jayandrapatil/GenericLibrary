using MyDbLib.Api.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyDbLib.Core.Resilience
{
    /// <summary>
    /// Default retry policy that performs no retry.
    /// Used when consumer does not configure retry.
    /// </summary>
    public sealed class NoRetryPolicy : IRetryPolicy
    {
        public static readonly NoRetryPolicy Instance = new();

        private NoRetryPolicy() { }

        public Task ExecuteAsync(Func<Task> action)
            => action();

        public Task<T> ExecuteAsync<T>(Func<Task<T>> action)
            => action();

        public T Execute<T>(Func<T> action)
            => action();
    }
}
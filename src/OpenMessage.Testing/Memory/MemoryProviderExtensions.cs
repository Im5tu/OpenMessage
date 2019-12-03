using OpenMessage;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Testing.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for adding an InMemory Dispatcher/Consumer
    /// </summary>
    public static class MemoryProviderExtensions
    {
        /// <summary>
        ///     Adds an InMemory <see cref="IDispatcher{T}" /> that will wait for the the message to be consumed before returning.
        ///     <see cref="AutoAcknowledgeMiddleware{T}" /> must be added to the pipeline for this function correctly
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T">The type add the dispatcher for</typeparam>
        public static IServiceCollection AddAwaitableMemoryDispatcher<T>(this IServiceCollection services) => services.AddSingleton<IDispatcher<T>, AwaitableMemoryDispatcher<T>>();
    }
}
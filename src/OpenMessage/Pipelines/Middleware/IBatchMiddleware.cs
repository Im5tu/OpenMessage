using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Middleware contract for a batched pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchMiddleware<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<T> next);
    }
}
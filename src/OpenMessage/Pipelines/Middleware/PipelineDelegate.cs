using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Delegates for representing pipeline middleware
    /// </summary>
    public static class PipelineDelegate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public delegate Task SingleMiddleware<T>(Message<T> message, CancellationToken cancellationToken, MessageContext context);

        /// <summary>
        ///
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public delegate Task BatchMiddleware<T>(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext context);
    }
}
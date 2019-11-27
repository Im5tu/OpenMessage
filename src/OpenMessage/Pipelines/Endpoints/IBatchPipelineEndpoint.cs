using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    /// Ends the batch pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchPipelineEndpoint<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext);
    }
}
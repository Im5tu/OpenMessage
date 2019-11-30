using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    ///     Ends the batch pipeline
    /// </summary>
    public interface IBatchPipelineEndpoint<T>
    {
        /// <summary>
        ///     Process a collection of messages
        /// </summary>
        Task Invoke(IReadOnlyCollection<Message<T>> messages, CancellationToken cancellationToken, MessageContext messageContext);
    }
}
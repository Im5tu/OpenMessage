using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    ///     Ends the batch pipeline
    /// </summary>
    public interface IPipelineEndpoint<T>
    {
        /// <summary>
        ///     Process a message
        /// </summary>
        Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext);
    }
}
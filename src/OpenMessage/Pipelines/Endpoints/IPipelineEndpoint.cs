using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Endpoints
{
    /// <summary>
    /// Ends the batch pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipelineEndpoint<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext);
    }
}

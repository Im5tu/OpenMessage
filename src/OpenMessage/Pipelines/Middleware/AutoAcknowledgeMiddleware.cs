using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Automatically acknowledges any messages that implement <see cref="ISupportAcknowledgement"/> at the end of the pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoAcknowledgeMiddleware<T> : IMiddleware<T>
    {
        private readonly ILogger<AutoAcknowledgeMiddleware<T>> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public AutoAcknowledgeMiddleware(ILogger<AutoAcknowledgeMiddleware<T>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="messageContext"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            try
            {
                await next(message, cancellationToken, messageContext);

                if (message is ISupportAcknowledgement acknowledgement)
                {
                    await acknowledgement.AcknowledgeAsync(positivelyAcknowledge: true);
                }
            }
            catch
            {
                if (message is ISupportAcknowledgement acknowledgement)
                {
                    await acknowledgement.AcknowledgeAsync(positivelyAcknowledge: false);
                }

                throw;
            }
        }
    }
}

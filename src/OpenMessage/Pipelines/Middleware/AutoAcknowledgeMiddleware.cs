using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Automatically acknowledges any messages that implement <see cref="ISupportAcknowledgement" /> at the end of the pipeline
    /// </summary>
    public class AutoAcknowledgeMiddleware<T> : Middleware<T>
    {
        /// <inheritDoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            try
            {
                await next(message, cancellationToken, messageContext);

                if (message is ISupportAcknowledgement acknowledgement)
                    await acknowledgement.AcknowledgeAsync();
            }
            catch (Exception e)
            {
                if (message is ISupportAcknowledgement acknowledgement)
                    await acknowledgement.AcknowledgeAsync(false, e);

                throw;
            }
        }
    }
}
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    /// Adds an activity trace and starts a logger scope
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TraceMiddleware<T> : IMiddleware<T>
    {
        private static readonly string ConsumeActivityName = "OpenMessage.Consumer.Process";

        /// <inheritdoc />
        public async Task Invoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            _ = TryGetActivityId(message, out var activityId);
            using (Trace.WithActivity(ConsumeActivityName, activityId))
            {
                await next(message, cancellationToken, messageContext);
            }
        }

        private static bool TryGetActivityId(Message<T> message, out string activityId)
        {
            activityId = null;

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        if (prop.Key == KnownProperties.ActivityId)
                        {
                            activityId = prop.Value;
                            return true;
                        }
                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        if (prop.Key == KnownProperties.ActivityId)
                        {
                            activityId = Encoding.UTF8.GetString(prop.Value);
                            return true;
                        }
                    break;
                }
            }

            return false;
        }
    }
}

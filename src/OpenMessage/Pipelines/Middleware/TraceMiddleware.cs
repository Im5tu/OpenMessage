using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Middleware
{
    /// <summary>
    ///     Adds an activity trace and starts a logger scope
    /// </summary>
    public class TraceMiddleware<T> : Middleware<T>
    {
        private static readonly string ConsumeActivityName = "OpenMessage.Consumer.Process";

        /// <inheritDoc />
        protected override async Task OnInvoke(Message<T> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<T> next)
        {
            if (TryGetActivityId(message, out var activityId))
                using (Trace.WithActivity(ConsumeActivityName, activityId))
                    await next(message, cancellationToken, messageContext);
            else
                await next(message, cancellationToken, messageContext);
        }

        private static bool TryGetActivityId(Message<T> message, [NotNullWhen(true)] out string? activityId)
        {
            activityId = null;

            switch (message)
            {
                case ISupportProperties p:
                {
                    if (p.Properties is IDictionary<string, string> dictionary)
                        if (dictionary.TryGetValue(KnownProperties.ActivityId, out activityId) && !string.IsNullOrWhiteSpace(activityId))
                            return true;
                        else
                        {
                            activityId = null; // Reset here because the value maybe whitespace
                            return false;
                        }

                    // Fallback for other versions that aren't a dictionary
                    foreach (var prop in p.Properties)
                        if (prop.Key == KnownProperties.ActivityId && !string.IsNullOrWhiteSpace(prop.Value))
                        {
                            activityId = prop.Value;
                            return true;
                        }

                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        if (prop.Key == KnownProperties.ActivityId && prop.Value?.Length > 0)
                        {
                            var id = Encoding.UTF8.GetString(prop.Value);
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                activityId = id;
                                return true;
                            }
                        }

                    break;
                }
            }

            return false;
        }
    }
}
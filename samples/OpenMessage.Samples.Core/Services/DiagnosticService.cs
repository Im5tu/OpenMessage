using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Samples.Core.Services
{
    internal sealed class DiagnosticService : EventListener, IHostedService
    {
        private readonly ILogger<DiagnosticService> _logger;
        private static readonly string EventCounterType = "CounterType";
        private static readonly string EventCountEventName = "EventCounters";
        private static readonly string EventName = "Name";
        private static readonly string IncrementName = "Increment";
        private static readonly string MeanType = "Mean";
        private static readonly string SumType = "Sum";

        public DiagnosticService(ILogger<DiagnosticService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventName == EventCountEventName
                && eventData.Payload.Count > 0
                && eventData.Payload[0] is IDictionary<string, object> data
                && data.TryGetValue(EventCounterType, out var counterType)
                && data.TryGetValue(EventName, out var name))
            {
                string metricName = name.ToString();
                string metricType = counterType.ToString();

                if (SumType.Equals(metricType) && data.TryGetValue(IncrementName, out var increment))
                {
                    _logger.LogInformation("{0}: {1}", metricName, increment);
                }
                else if (MeanType.Equals(metricType) && data.TryGetValue(MeanType, out var mean))
                {
                    _logger.LogInformation("{0}: {1}", metricName, mean);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            EnableEvents(OpenMessageEventSource.Instance, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string> {{"EventCounterIntervalSec", "1"}});
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
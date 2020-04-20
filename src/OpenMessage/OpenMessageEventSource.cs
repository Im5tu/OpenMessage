#if NETCOREAPP3_1
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

namespace OpenMessage
{
    // Inspo: https://github.com/aspnet/AspNetCore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/Internal/HostingEventSource.cs
    [EventSource(Name = "OpenMessage")]
    internal class OpenMessageEventSource : EventSource
    {
        internal static readonly OpenMessageEventSource Instance = new OpenMessageEventSource();

        private long _inflightMessages = 0;
        private long _processedCount = 0;
        private IncrementingPollingCounter? _inflightMessagesCounter;
        private EventCounter? _messageDurationCounter;
        private IncrementingPollingCounter? _processedCountCounter;

        private OpenMessageEventSource() { }

        // NOTE
        // - The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They enable creating 'activities'.
        //   For more information, take a look at the following blog post: https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/
        // - A stop event's event id must be next one after its start event.
        // - Avoid renaming methods or parameters marked with EventAttribute. EventSource uses these to form the event object.
        [NonEvent]
        public ValueStopwatch? ProcessMessageStart()
        {
            if (!IsEnabled()) return null;

            MessageStart();

            return ValueStopwatch.StartNew();
        }

        [Event(1, Level = EventLevel.Informational, Message = "Consumed Message")]
        private void MessageStart()
        {
            Interlocked.Increment(ref _inflightMessages);
            Interlocked.Increment(ref _processedCount);
        }

        [NonEvent]
        public void ProcessMessageStop(ValueStopwatch stopwatch)
        {
            if (!IsEnabled()) return;

            MessageStop(stopwatch.IsActive ? stopwatch.GetElapsedTime().TotalMilliseconds : 0.0);
        }

        [Event(2, Level = EventLevel.Informational, Message = "Message Completed")]
        private void MessageStop(double duration)
        {
            Interlocked.Decrement(ref _inflightMessages);
            _messageDurationCounter?.WriteMetric(duration);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                // This is the convention for initializing counters in the RuntimeEventSource (lazily on the first enable command).
                // They aren't disabled afterwards...
                _inflightMessagesCounter ??= new IncrementingPollingCounter("inflight-messages", this, () => _inflightMessages)
                {
                    DisplayName = "Inflight Messages",
                    DisplayUnits = "Messages"
                };
                _messageDurationCounter ??= new EventCounter("message-duration", this)
                {
                    DisplayName = "Average Message Duration",
                    DisplayUnits = "ms"
                };
                _processedCountCounter ??= new IncrementingPollingCounter("processed-count", this, () => _processedCount)
                {
                    DisplayName = "Messages Processed",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
            }
        }

        // inspo: https://github.com/aspnet/Extensions/blob/34204b6bc41de865f5310f5f237781a57a83976c/src/Shared/src/ValueStopwatch/ValueStopwatch.cs
        internal struct ValueStopwatch
        {
            private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
            private long _startTimestamp;
            public bool IsActive => _startTimestamp != 0;

            private ValueStopwatch(long startTimestamp)
            {
                _startTimestamp = startTimestamp;
            }

            public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

            public TimeSpan GetElapsedTime()
            {
                // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
                // So it being 0 is a clear indication of default(ValueStopwatch)
                if (!IsActive)
                {
                    ThrowUninitializedException();
                }

                var end = Stopwatch.GetTimestamp();
                var timestampDelta = end - _startTimestamp;
                var ticks = (long)(TimestampToTicks * timestampDelta);
                return new TimeSpan(ticks);
            }

            private void ThrowUninitializedException()
                => throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
        }
    }
}
#endif

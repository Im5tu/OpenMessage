using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;

namespace OpenMessage.Apache.Kafka
{
    internal abstract class KafkaClient
    {
        protected ILogger Logger { get; }

        protected KafkaClient(ILogger logger) => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        protected virtual void Kafka_OnError(IClient client, Error error)
        {
            if (error is null)
                return;

            OnLog(error.IsFatal ? SyslogLevel.Alert : SyslogLevel.Error, $"{error.Code} - {error.Reason} (Local: {error.IsLocalError} IsBrokerError: {error.IsBrokerError})");
        }

        protected virtual void Kafka_OnLog(object sender, LogMessage message)
        {
            if (message is null)
                return;

            OnLog(message.Level, $"{message.Facility}:{message.Name} - {message.Message}");
        }

        protected virtual void Kafka_OnStatistics(object sender, string e)
        {
            if (e is null)
                return;

            OnLog(SyslogLevel.Debug, e);
        }

        protected virtual void OnLog(SyslogLevel level, string message)
        {
            switch (level)
            {
                case SyslogLevel.Emergency:
                case SyslogLevel.Alert:
                case SyslogLevel.Critical:
                    Logger.LogCritical(message);

                    break;
                case SyslogLevel.Error:
                    Logger.LogError(message);

                    break;
                case SyslogLevel.Warning:
                    Logger.LogWarning(message);

                    break;
                case SyslogLevel.Notice:
                case SyslogLevel.Info:
                    Logger.LogInformation(message);

                    break;
                default:
                    Logger.LogDebug(message);

                    break;
            }
        }
    }
}
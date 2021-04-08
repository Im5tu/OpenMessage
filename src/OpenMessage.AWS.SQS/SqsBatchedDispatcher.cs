using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;
using OpenMessage.Serialization;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsBatchedDispatcher<T> : DispatcherBase<T>
    {
        //15min = 900sec is the maximum delay supported by sqs
        private const int MaximumSqsDelaySeconds = 900;
        private static readonly string AttributeType = "String";
        private readonly MessageAttributeValue _contentType;
        private readonly IOptionsMonitor<SQSDispatcherOptions<T>> _options;
        private readonly ISerializer _serializer;
        private readonly ChannelWriter<SendSqsMessageCommand> _messageWriter;

        public SqsBatchedDispatcher(IOptionsMonitor<SQSDispatcherOptions<T>> options, ISerializer serializer, ILogger<SqsDispatcher<T>> logger, ChannelWriter<SendSqsMessageCommand> messageWriter)
            : base(logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));

            _contentType = new MessageAttributeValue
            {
                DataType = AttributeType,
                StringValue = _serializer.ContentType
            };
        }

        public override async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            if (message.Value is null)
                Throw.Exception("Message value cannot be null");

            var json = _serializer.AsString(message.Value);
            if (string.IsNullOrWhiteSpace(json))
                Throw.Exception("Message could not be serialized");

            var options = _options.CurrentValue;
            if (options is null)
                Throw.Exception("Options cannot be null");

            LogDispatch(message);

            var request = new SendMessageBatchRequestEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                MessageAttributes = GetMessageProperties(message),
                MessageBody = json
            };

            var delay = DelaySeconds(message);
            if (delay.HasValue)
                request.DelaySeconds = delay.Value;

            var msg = new SendSqsMessageCommand
            {
                Message = request,
                QueueUrl = options.QueueUrl,
                ServiceUrl = options.ServiceURL,
                RegionEndpoint = options.RegionEndpoint
            };

            await _messageWriter.WriteAsync(msg, cancellationToken);

            var taskCancellation = cancellationToken.Register(() => msg.Cancel(cancellationToken));
            try
            {
                await msg.WaitForCompletion();
            }
            finally
            {
                taskCancellation.Dispose();
            }
        }

        private static int? DelaySeconds(Message<T> message)
        {
            if (message is ISupportSendDelay delay && delay.SendDelay > TimeSpan.Zero)
            {
                return Math.Min(MaximumSqsDelaySeconds, (int) delay.SendDelay.TotalSeconds);
            }

            return null;
        }

        private Dictionary<string, MessageAttributeValue> GetMessageProperties(Message<T> message)
        {
            var result = new Dictionary<string, MessageAttributeValue>
            {
                [KnownProperties.ContentType] = _contentType
            };

            if (!(message.Value is null))
                result[KnownProperties.ValueTypeName] = new MessageAttributeValue
                {
                    DataType = AttributeType,
                    StringValue = message.Value.GetType().AssemblyQualifiedName
                };

            if (Activity.Current is {})
                result[KnownProperties.ActivityId] = new MessageAttributeValue
                {
                    DataType = AttributeType,
                    StringValue = Activity.Current.Id
                };

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        result[prop.Key] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = prop.Value
                        };

                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        result[prop.Key] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = Encoding.UTF8.GetString(prop.Value)
                        };

                    break;
                }
                case ISupportProperties<byte[], byte[]> p3:
                {
                    foreach (var prop in p3.Properties)
                        result[Encoding.UTF8.GetString(prop.Key)] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = Encoding.UTF8.GetString(prop.Value)
                        };

                    break;
                }
            }

            return result;
        }
    }
}
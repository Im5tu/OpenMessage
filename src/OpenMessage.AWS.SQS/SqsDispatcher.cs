using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;
using OpenMessage.Serialisation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsDispatcher<T> : DispatcherBase<T>
    {
        private static readonly string AttributeType = "String";
        private readonly MessageAttributeValue _contentType;
        private readonly string _queueUrl;
        private readonly ISerializer _serializer;
        private readonly ChannelWriter<SendMessage> _messageWriter;

        public SqsDispatcher(IOptions<SQSDispatcherOptions<T>> options, ISerializer serializer, ILogger<SqsDispatcher<T>> logger, ChannelWriter<SendMessage> messageWriter)
            : base(logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _queueUrl = config.QueueUrl;

            _contentType = new MessageAttributeValue
            {
                DataType = AttributeType,
                StringValue = _serializer.ContentType
            };
        }

        public override async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            var request = new SendMessageBatchRequestEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                MessageAttributes = GetMessageProperties(message),
                MessageBody = _serializer.AsString(message.Value)
            };

            var msg = new SendMessage
            {
                Message = request,
                QueueUrl = _queueUrl
            };

            await _messageWriter.WriteAsync(msg, cancellationToken);

            var taskCancellation = cancellationToken.Register(() => msg.TCS.TrySetCanceled());
            try
            {
                await msg.TCS.Task;
            }
            finally
            {
                taskCancellation.Dispose();
            }
        }

        private Dictionary<string, MessageAttributeValue> GetMessageProperties(Message<T> message)
        {
            var result = new Dictionary<string, MessageAttributeValue>
            {
                [KnownProperties.ContentType] = _contentType,
                [KnownProperties.ValueTypeName] = new MessageAttributeValue
                {
                    DataType = AttributeType,
                    StringValue = message.Value.GetType().AssemblyQualifiedName
                }
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
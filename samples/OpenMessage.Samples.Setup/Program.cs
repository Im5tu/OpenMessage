using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage.Samples.Setup
{
    internal class Program
    {
        private static readonly string topic = "OpenMessage.Samples.Core.Models.SimpleModel".ToLowerInvariant();

        private static async Task Main(string[] args)
        {
            await Task.WhenAll(SetupKafka(), SetupAws());
        }

        private static async Task SetupAws()
        {
            try
            {
                Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "XXX", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "XXX", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "XXX", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", "us-east-1", EnvironmentVariableTarget.Process);

                var snsClient = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
                {
                    ServiceURL = "http://localhost:4575"
                });

                var sqsClient = new AmazonSQSClient(new AmazonSQSConfig
                {
                    ServiceURL = "http://localhost:4576"
                });

                var topicName = topic.Replace(".", "_");

                var topicRequest = new CreateTopicRequest(topicName);
                var topicResponse = await snsClient.CreateTopicAsync(topicRequest);

                var queueRequest = new CreateQueueRequest($"{topicName}.queue");
                var queueResponse = await sqsClient.CreateQueueAsync(queueRequest);

                var subscribeRequest = new SubscribeRequest
                {
                    Endpoint = queueResponse.QueueUrl,
                    TopicArn = topicResponse.TopicArn,
                    Protocol = "sqs",
                    ReturnSubscriptionArn = true,
                    Attributes = new Dictionary<string, string>
                    {
                        ["RawMessageDelivery"] = "true"
                    }
                };
                var subscribeResponse = await snsClient.SubscribeAsync(subscribeRequest);

                (await snsClient.ListTopicsAsync()).Topics.ForEach(x => Console.WriteLine($"[AWS] Topic: {x.TopicArn}"));
                (await sqsClient.ListQueuesAsync(new ListQueuesRequest())).QueueUrls.ForEach(x => Console.WriteLine($"[AWS] Queue: {x}"));
                (await snsClient.ListSubscriptionsAsync(new ListSubscriptionsRequest())).Subscriptions.ForEach(x => Console.WriteLine($"[AWS] Subscription: {x.TopicArn} -> {x.Endpoint}"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AWS] {e.Message}");
            }
        }

        private static async Task SetupKafka()
        {
            try
            {
                var client = new AdminClientBuilder(new Dictionary<string, string>
                    {
                        ["bootstrap.servers"] = "localhost:9092",
                        ["topic.metadata.refresh.interval.ms"] = "500"
                    }).SetLogHandler((client, message) => Console.WriteLine($"[Kafka] [{message.Level}] {message.Message}"))
                      .Build();

                var topics = client.GetMetadata(TimeSpan.FromMinutes(1))
                                   .Topics;

                if (topics.Any(x => x.Topic.StartsWith("OpenMessage", StringComparison.OrdinalIgnoreCase)))
                {
                    await client.DeleteTopicsAsync(topics.Where(x => x.Topic.StartsWith("OpenMessage", StringComparison.OrdinalIgnoreCase))
                                                         .Select(x => x.Topic));

                    await Task.Delay(1000);
                }

                await client.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = 5,
                        ReplicationFactor = 1,
                        Configs = new Dictionary<string, string>
                        {
                            ["retention.ms"] = TimeSpan.FromMinutes(15)
                                                       .Milliseconds.ToString()
                        }
                    }
                });
                await Task.Delay(1000);

                topics = client.GetMetadata(TimeSpan.FromMinutes(1))
                               .Topics;
                topics.ForEach(x => Console.WriteLine($"[Kafka] Topic: {x.Topic}(Partitions: {x.Partitions.Count})"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Kafka] {e.Message}");
            }
        }
    }
}
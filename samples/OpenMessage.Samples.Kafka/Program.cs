using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenMessage.Samples.Kafka
{
    internal class Program
    {
        private static int _counter;

        private static async Task Main()
        {
            await Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                        services.AddOptions()
                            .AddLogging()
                            .AddMassProducerService<Dictionary<string, string>>() // Adds a producer that calls configured dispatcher
                )
                .ConfigureMessaging(host =>
                {
                    // Adds a handler that writes to console every 1000 messages
                    host.ConfigureHandler<Dictionary<string, string>>(msg =>
                    {
                        var counter = Interlocked.Increment(ref _counter);
                        if (counter % 1000 == 0)
                            Console.WriteLine("Counter: " + counter);
                    });

                    // Allow us to write to kafka
                    host.ConfigureKafkaDispatcher<Dictionary<string, string>>(options =>
                    {
                        options.TopicName = "logs";
                    });

                    // Consume from the same topic as we are writing to
                    host.ConfigureKafkaConsumer<Dictionary<string, string>>()
                        .FromTopic("logs")
                        .Build();
                })
                .Build()
                .RunAsync();
        }
    }
}
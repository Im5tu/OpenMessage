using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Memory;
using OpenMessage.Pipelines;
using OpenMessage.Samples.Core.Models;

namespace OpenMessage.Samples.Batch
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
                            .AddMassProducerService<SimpleModel>() // Adds a producer that calls configured dispatcher
                )
                .ConfigureMessaging(host =>
                {
                    // Adds a handler that writes to console every 1000 messages
                    host.ConfigureBatchHandler<SimpleModel>(batch =>
                    {
                        var counter = Interlocked.Add(ref _counter, batch.Count);
                        Console.WriteLine($"Counter: {counter}. Batch Size: {batch.Count}");
                    });

                    host.ConfigureMemoryConsumer<SimpleModel>().Build();
                    host.ConfigureMemoryDispatcher<SimpleModel>().Build();
                })
                .Build()
                .RunAsync();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Samples.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Samples.Kafka
{
    internal class Program
    {
        private static int _counter;

        private static async Task Main()
        {
            await Host.CreateDefaultBuilder()
                      .ConfigureServices(services => services.AddOptions()
                                                             .AddLogging()
                                                             .AddSampleCore()
                                                             .AddMassProducerService<SimpleModel>() // Adds a producer that calls configured dispatcher
                      )
                      .ConfigureMessaging(host =>
                      {
                          // Adds a handler that writes to console every 1000 messages
                          host.ConfigureHandler<SimpleModel>(msg =>
                          {
                              var counter = Interlocked.Increment(ref _counter);

                              if (counter % 1000 == 0)
                                  Console.WriteLine($"Counter: {counter}");
                          });

                          // Allow us to write to kafka
                          host.ConfigureKafkaDispatcher<SimpleModel>(options => { });

                          // Consume from the same topic as we are writing to
                          host.ConfigureKafkaConsumer<SimpleModel>()
                              .Build();
                      })
                      .Build()
                      .RunAsync();
        }
    }
}
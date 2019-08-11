using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace OpenMessage.Samples.Memory
{
    internal class Program
    {
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
                    // Adds a memory based consumer and dispatcher
                    host.ConfigureMemory<Dictionary<string, string>>();

                    // Add a handler that writes the entire message in json format to the console
                    host.ConfigureHandler<Dictionary<string, string>>(msg => Console.WriteLine(JsonConvert.SerializeObject(msg.Value)));
                })
                .Build()
                .RunAsync();
        }
    }
}
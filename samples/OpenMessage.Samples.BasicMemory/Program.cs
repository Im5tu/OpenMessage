using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace OpenMessage.Samples.BasicMemory
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OpenMessage memory sample running. Press any key to stop...");
            var cts = new CancellationTokenSource();

            var services = new ServiceCollection()
                                    .AddLogging()
                                    .AddMemoryChannel<string>()
                                    .AddObserver<string>(Console.WriteLine)
                                    .BuildServiceProvider();

            var brokers = services.GetRequiredService<IEnumerable<IBroker>>().ToList();
            var dispatcher = services.GetRequiredService<IDispatcher<string>>();
            var task = Task.Run(async () =>
            {
                while(!cts.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    await dispatcher.DispatchAsync(DateTime.UtcNow.ToString());
                }
            });

            Console.ReadKey();
            cts.Cancel();
            task.Wait();
        }
    }
}
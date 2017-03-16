using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

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

            using (services.GetRequiredService<IBrokerHost>())
            {
                var dispatcher = services.GetRequiredService<IDispatcher<string>>();
                var task = Task.Run(async () =>
                {
                    while (!cts.IsCancellationRequested)
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
}
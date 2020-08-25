using AutoFixture;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Samples.Core.Services
{
    internal sealed class MassProducerService<T> : BackgroundService
        where T : new()
    {
        private readonly IDispatcher<T> _dispatcher;
        private readonly Fixture _fixture = new Fixture();
        private const int DispatchBatchSize = 100;

        public MassProducerService(IDispatcher<T> dispatcher) => _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(Enumerable.Range(1, DispatchBatchSize)
                    .Select(async x =>
                    {
                        try
                        {
                            await _dispatcher.DispatchAsync(new ExtendedMessage<T>(_fixture.Create<T>())
                            {
                                //SendDelay = TimeSpan.FromSeconds(15),
                                Properties = new List<KeyValuePair<string, string>>
                                {
                                    new KeyValuePair<string, string>("Dispatched", DateTime.UtcNow.ToString())
                                }
                            }, stoppingToken);
                        }
                        catch (Exception e)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                return;

                            Console.WriteLine("MassProducer: " + e.Message);
                        }
                    }));

                Console.WriteLine($"Dispatched: {DispatchBatchSize}");
            }
        }
    }
}
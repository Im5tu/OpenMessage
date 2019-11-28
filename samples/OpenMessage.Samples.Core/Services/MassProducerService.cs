using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Hosting;

namespace OpenMessage.Samples.Core.Services
{
    internal sealed class MassProducerService<T> : BackgroundService
        where T : new()
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDispatcher<T> _dispatcher;

        public MassProducerService(IDispatcher<T> dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(100);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(Enumerable.Range(1, 100).Select(async x =>
                {
                    try
                    {
                        await _dispatcher.DispatchAsync(_fixture.Create<T>());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }));
            }
        }
    }
}
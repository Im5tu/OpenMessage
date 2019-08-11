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
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(Enumerable.Range(1, 5000 * 5).Select(x => _dispatcher.DispatchAsync(_fixture.Create<T>())));

                await Task.Delay(100);
            }
        }
    }
}
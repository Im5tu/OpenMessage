using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Hosting;

namespace OpenMessage.Samples.Core.Services
{
    internal sealed class ProducerService<T> : BackgroundService
        where T : new()
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDispatcher<T> _dispatcher;

        public ProducerService(IDispatcher<T> dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) await _dispatcher.DispatchAsync(_fixture.Create<T>());
        }
    }
}
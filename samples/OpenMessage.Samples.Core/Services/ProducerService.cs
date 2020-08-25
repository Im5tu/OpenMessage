using AutoFixture;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Samples.Core.Services
{
    internal sealed class ProducerService<T> : BackgroundService
        where T : new()
    {
        private readonly IDispatcher<T> _dispatcher;
        private readonly Fixture _fixture = new Fixture();

        public ProducerService(IDispatcher<T> dispatcher) => _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    await _dispatcher.DispatchAsync(_fixture.Create<T>(), stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Producer: " + e.Message);
                }
                finally
                {
                    await Task.Delay(1000);
                }
        }
    }
}
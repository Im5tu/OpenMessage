using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;
using Xunit;

namespace OpenMessage.Testing.Tests.Memory
{
    public class MemoryTests : IDisposable, IAsyncLifetime
    {
        private IHost _app;
        private bool _finished;
        private readonly IHostBuilder _hostBuilder;

        public MemoryTests()
        {
            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureMessaging(builder =>
                {
                    builder
                        .ConfigureMemory<string>()
                        .Build();

                    builder.ConfigurePipeline<string>()
                        .Use<AutoAcknowledgeMiddleware<string>>()
                        .Run(message =>
                        {
                            _finished = true;
                            return Task.CompletedTask;
                        });
                });

        }

        [Fact]
        public async Task WhenAwaitableMemoryDispatcherIsAdded_ThenAwaitingTheDispatchOfTheEventWillWaitForTheConsumerToFinish()
        {
            _app = _hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddAwaitableMemoryDispatcher<string>();
                })
                .Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            Assert.True(_finished);
        }

        [Fact]
        public async Task WhenAwaitableMemoryDispatcherIsNotAdded_ThenAwaitingTheDispatchOfTheEventWillNotWaitForTheConsumerToFinish()
        {
            _app = _hostBuilder.Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            Assert.False(_finished);
        }

        public void Dispose() => _app?.Dispose();
        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => _app.StopAsync();
    }
}
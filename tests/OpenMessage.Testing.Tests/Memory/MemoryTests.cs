using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Pipelines.Builders;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OpenMessage.Testing.Tests.Memory
{
    public class MemoryTests : IDisposable, IAsyncLifetime
    {
        private readonly IHostBuilder _hostBuilder;

        private IHost _app;
        private bool _finished;

        public MemoryTests()
        {
            _hostBuilder = Host.CreateDefaultBuilder()
                               .ConfigureMessaging(builder =>
                               {
                                   builder.ConfigureMemory<string>()
                                          .Build();

                                   builder.ConfigurePipeline<string>()
                                          .UseDefaultMiddleware()
                                          .Run(async message =>
                                          {
                                              await Task.Delay(1000);
                                              _finished = true;
                                          });
                               });
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        public Task DisposeAsync() => _app.StopAsync();

        public Task InitializeAsync() => Task.CompletedTask;

        [Fact]
        public async Task WhenAwaitableMemoryDispatcherIsAdded_ThenAwaitingTheDispatchOfTheEventWillWaitForTheConsumerToFinish()
        {
            _app = _hostBuilder.ConfigureServices(services =>
                               {
                                   services.AddAwaitableMemoryDispatcher<string>();
                               })
                               .Build();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>()
                      .DispatchAsync("");

            Assert.True(_finished);
        }

        [Fact]
        public async Task WhenAwaitableMemoryDispatcherIsNotAdded_ThenAwaitingTheDispatchOfTheEventWillNotWaitForTheConsumerToFinish()
        {
            _app = _hostBuilder.Build();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>()
                      .DispatchAsync("");

            Assert.False(_finished);
        }
    }
}
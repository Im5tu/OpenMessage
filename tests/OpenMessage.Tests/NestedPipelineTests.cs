using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class NestedPipelineTests : IDisposable, IAsyncLifetime
    {
        private readonly IList<string> _history = new List<string>();
        private readonly IHostBuilder _host;

        private IHost _app;

        public NestedPipelineTests(ITestOutputHelper testOutputHelper)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton(_history);
                })
                .ConfigureLogging(builder => builder.AddTestOutputHelper(testOutputHelper))
                .ConfigureMessaging(builder =>
                {
                    builder.ConfigureMemory<string>()
                        .Build();

                    builder.ConfigurePipeline<string>()
                        .UseDefaultMiddleware()
                        .Batch()
                        .Run(async (messages, cancellationToken, context) =>
                        {
                            var message = messages.Single().Value;

                            _history.Add($"Start {message}");

                            if (message == "Hello")
                            {
                                var dispatcher = context.ServiceProvider.GetRequiredService<IDispatcher<string>>();

                                await dispatcher.DispatchAsync("World");
                            }


                            _history.Add($"End {message}");
                        });
                })
                .ConfigureServices(services =>
                {
                    services.AddAwaitableMemoryDispatcher<string>();
                });
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        public Task DisposeAsync() => _app?.StopAsync();

        public Task InitializeAsync() => Task.CompletedTask;

        [Fact]
        public async Task WhenAHandlerDispatchesAnEvent_ThenThatEventCanBeHandled()
        {
            _app = _host.Build();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("Hello");


            var i = 0;

            Assert.Equal("Start Hello", _history[i++]);
            Assert.Equal("Start World", _history[i++]);
            Assert.Equal("End World", _history[i++]);
            Assert.Equal("End Hello", _history[i++]);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class PipelineTests : IDisposable, IAsyncLifetime
    {
        private IHost _app;
        private readonly IList<string> _history = new List<string>();
        private Func<Message<string>, Task> _run;
        private readonly IHostBuilder _host;

        public PipelineTests(ITestOutputHelper testOutputHelper)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton<CustomMiddleware>();
                    services.AddSingleton(_history);
                })
                .ConfigureLogging(builder => builder.AddTestOutputHelper(testOutputHelper))
                .ConfigureMessaging(builder =>
                {
                    builder
                        .ConfigureMemory<string>()
                        .Build();

                    builder.ConfigurePipeline<string>()
                        .UseDefaultMiddleware()
                        .Use<CustomMiddleware>()
                        .Use(async (message, next) =>
                        {
                            _history.Add("Func");
                            await next();
                            _history.Add("Func");
                        })
                        .Run(async message =>
                        {
                            _history.Add("Run");
                            if (_run != null)
                            {
                                await _run(message);
                            }
                        });
                })
                .ConfigureServices(services =>
                {
                    services.AddAwaitableMemoryDispatcher<string>();
                });
        }

        [Fact]
        public async Task WhenAnExceptionIsThrown_ThenTheMessageIsNotPositivelyAcknowledged()
        {
            _app = _host.Build();
            _run = messages => throw new Exception();

            var message = new CustomMessage();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync(message);

            Assert.Equal(AcknowledgementState.NegativelyAcknowledged, message.AcknowledgementState);
        }

        [Fact]
        public async Task MiddlewareAndRunAreExecutedInTheCorrectOrder()
        {
            _app = _host.Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            var i = 0;

            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal("Run", _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
        }

        public void Dispose() => _app?.Dispose();
        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => _app?.StopAsync();

        private class CustomMessage : Message<string>, ISupportAcknowledgement
        {
            public AcknowledgementState AcknowledgementState { get; private set; }

            public Task AcknowledgeAsync(bool positivelyAcknowledge = true)
            {
                AcknowledgementState = positivelyAcknowledge
                    ? AcknowledgementState.Acknowledged
                    : AcknowledgementState.NegativelyAcknowledged;

                return Task.CompletedTask;
            }

        }

        private class CustomMiddleware : IMiddleware<string>
        {
            private readonly ILogger<CustomMiddleware> _logger;
            private readonly IList<string> _history;

            public CustomMiddleware(ILogger<CustomMiddleware> logger, IList<string> history)
            {
                _logger = logger;
                _history = history;
            }

            public async Task Invoke(Message<string> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<string> next)
            {
                _logger.LogInformation($"Before {nameof(CustomMiddleware)}");
                _history.Add(nameof(CustomMiddleware));

                await next(message, cancellationToken, messageContext);

                _history.Add(nameof(CustomMiddleware));
                _logger.LogInformation($"After {nameof(CustomMiddleware)}");
            }
        }
    }
}

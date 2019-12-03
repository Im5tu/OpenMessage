using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class PipelineTests : IDisposable, IAsyncLifetime
    {
        private readonly IList<string> _history = new List<string>();
        private readonly IHostBuilder _host;

        private IHost _app;
        private Func<Message<string>, Task> _run;

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
                            builder.ConfigureMemory<string>()
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

                                       if (_run is {})
                                           await _run(message);
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
        public async Task MiddlewareAndRunAreExecutedInTheCorrectOrder()
        {
            _app = _host.Build();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>()
                      .DispatchAsync("");

            var i = 0;

            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal("Run", _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
        }

        [Fact]
        public async Task WhenAnExceptionIsThrown_ThenTheMessageIsNotPositivelyAcknowledged()
        {
            _app = _host.Build();
            _run = messages => throw new Exception();

            var message = new CustomMessage();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>()
                      .DispatchAsync(message);

            Assert.Equal(AcknowledgementState.NegativelyAcknowledged, message.AcknowledgementState);
        }

        private class CustomMessage : Message<string>, ISupportAcknowledgement
        {
            public AcknowledgementState AcknowledgementState { get; private set; }

            public Task AcknowledgeAsync(bool positivelyAcknowledge = true)
            {
                AcknowledgementState = positivelyAcknowledge ? AcknowledgementState.Acknowledged : AcknowledgementState.NegativelyAcknowledged;

                return Task.CompletedTask;
            }
        }

        private class CustomMiddleware : IMiddleware<string>
        {
            private readonly IList<string> _history;
            private readonly ILogger<CustomMiddleware> _logger;

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
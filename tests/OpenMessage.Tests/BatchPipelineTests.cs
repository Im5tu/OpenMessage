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
    public class BatchPipelineTests : IDisposable, IAsyncLifetime
    {
        private IHost _app;
        private readonly IList<string> _history = new List<string>();
        private readonly IHostBuilder _host;
        private Func<IReadOnlyCollection<Message<string>>, Task> _run;

        public BatchPipelineTests(ITestOutputHelper testOutputHelper)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton<CustomBatchMiddleware>();
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
                        .Batch()
                        .Use<CustomBatchMiddleware>()
                        .Use(async (messages, next) =>
                        {
                            _history.Add("BatchFunc");
                            await next();
                            _history.Add("BatchFunc");
                        })
                        .Run(async messages =>
                        {
                            _history.Add("Run");

                            if (_run != null)
                            {
                                await _run(messages);
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
        public async Task BatchMiddlewareAndRunAreExecutedInTheCorrectOrder()
        {
            _app = _host.Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            var i = 0;

            Assert.Equal(nameof(CustomBatchMiddleware), _history[i++]);
            Assert.Equal("BatchFunc", _history[i++]);
            Assert.Equal("Run", _history[i++]);
            Assert.Equal("BatchFunc", _history[i++]);
            Assert.Equal(nameof(CustomBatchMiddleware), _history[i++]);
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

        private class CustomBatchMiddleware : IBatchMiddleware<string>
        {
            private readonly ILogger<CustomBatchMiddleware> _logger;
            private readonly IList<string> _history;

            public CustomBatchMiddleware(ILogger<CustomBatchMiddleware> logger, IList<string> history)
            {
                _logger = logger;
                _history = history;
            }

            public async Task Invoke(IReadOnlyCollection<Message<string>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<string> next)
            {
                _logger.LogInformation($"Before {nameof(CustomBatchMiddleware)}");
                _history.Add(nameof(CustomBatchMiddleware));

                await next(messages, cancellationToken, messageContext);

                _history.Add(nameof(CustomBatchMiddleware));
                _logger.LogInformation($"After {nameof(CustomBatchMiddleware)}");
            }
        }
    }
}
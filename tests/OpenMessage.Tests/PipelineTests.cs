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
        private readonly IHost _app;
        private readonly IList<string> _history = new List<string>();

        public PipelineTests(ITestOutputHelper testOutputHelper)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton<CustomBatchMiddleware>();
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
                        .Use<AutoAcknowledgeMiddleware<string>>()
                        .Use<CustomMiddleware>()
                        .Use(async (message, next) =>
                        {
                            _history.Add("Func");
                            await next();
                            _history.Add("Func");
                        })
                        .Batch()
                        .Use<CustomBatchMiddleware>()
                        .Use(async (messages, next) =>
                        {
                            _history.Add("BatchFunc");
                            await next();
                            _history.Add("BatchFunc");
                        })
                        .Run(messages =>
                        {
                            _history.Add("Run");
                            return Task.CompletedTask;
                        });
                })
                .ConfigureServices(services =>
                {
                    services.AddAwaitableMemoryDispatcher<string>();
                });

            _app = host.Build();
        }

        [Fact]
        public async Task MiddlewareBatchMiddlewareAndRunAreExecutedInTheCorrectOrder()
        {
            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            var i = 0;

            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal(nameof(CustomBatchMiddleware), _history[i++]);
            Assert.Equal("BatchFunc", _history[i++]);
            Assert.Equal("Run", _history[i++]);
            Assert.Equal("BatchFunc", _history[i++]);
            Assert.Equal(nameof(CustomBatchMiddleware), _history[i++]);
            Assert.Equal("Func", _history[i++]);
            Assert.Equal(nameof(CustomMiddleware), _history[i++]);
        }

        public void Dispose() => _app?.Dispose();
        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => _app?.StopAsync();

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

        private class CustomBatchMiddleware : IBatchMiddleware<string>
        {
            private readonly ILogger<CustomMiddleware> _logger;
            private readonly IList<string> _history;

            public CustomBatchMiddleware(ILogger<CustomMiddleware> logger, IList<string> history)
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

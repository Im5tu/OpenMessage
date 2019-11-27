using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMessage.Handlers;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class UnitTest1 : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IHost _app;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton<MyBatchMiddleware>();
                    services.AddSingleton<MyMiddleware>();
                })
                .ConfigureLogging(builder => builder.AddTestOutputHelper(testOutputHelper))
                .ConfigureMessaging(builder =>
                {
                    builder
                        .ConfigureMemory<string>()
                        .ConfigureOptions(options =>
                        {
                            options.FireAndForget = false;
                        })
                        .Build();

                    builder
                        .ConfigureHandler<string, StringHandler>()
                        .ConfigureBatchHandler<string, StringHandler>();

                    builder.ConfigurePipeline<string>(options =>
                        {
                            options.PipelineType = PipelineType.Serial;
                        })
                        .Use<AutoAcknowledgeMiddleware<string>>()
                        .Use<MyMiddleware>()
                        .Batch()
                        .Use<MyBatchMiddleware>();
                });

            _app = host.Build();
        }

        [Fact]
        public async Task Test1()
        {
            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("1");

            _testOutputHelper.WriteLine("Done");
        }

        private class MyMiddleware : IMiddleware<string>
        {
            private readonly ILogger<MyMiddleware> _logger;

            public MyMiddleware(ILogger<MyMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Invoke(Message<string> message, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.SingleMiddleware<string> next)
            {
                _logger.LogInformation($"Before {nameof(MyMiddleware)}");

                await Task.Delay(1000);
                await next(message, cancellationToken, messageContext);

                _logger.LogInformation($"After {nameof(MyMiddleware)}");
            }
        }

        private class MyBatchMiddleware : IBatchMiddleware<string>
        {
            private readonly ILogger<MyMiddleware> _logger;

            public MyBatchMiddleware(ILogger<MyMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Invoke(IReadOnlyCollection<Message<string>> messages, CancellationToken cancellationToken, MessageContext messageContext, PipelineDelegate.BatchMiddleware<string> next)
            {
                _logger.LogInformation($"Before {nameof(MyBatchMiddleware)}");

                await next(messages, cancellationToken, messageContext);

                _logger.LogInformation($"After {nameof(MyBatchMiddleware)}");
            }
        }

        public void Dispose()
        {
            _app?.Dispose();
        }
    }

    public class StringHandler : IHandler<string>, IBatchHandler<string>
    {
        private readonly ILogger<StringHandler> _logger;

        public StringHandler(ILogger<StringHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(Message<string> message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handler: {message.Value}");

            return Task.CompletedTask;
        }

        public Task HandleAsync(IReadOnlyCollection<Message<string>> messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                _logger.LogInformation($"Batch Handler: {message.Value}");
            }

            return Task.CompletedTask;
        }
    }
}

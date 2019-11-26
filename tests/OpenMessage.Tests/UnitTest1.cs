using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMessage.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

        [Fact]
        public async Task Test1()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton(typeof(ShittyBatcher<>));
                    services.AddSingleton<MyBatchMiddleware>();
                    services.AddSingleton<MyMiddleware>();
                })
                .ConfigureLogging(builder => builder.AddTestOutputHelper(_testOutputHelper))
                .ConfigureMessaging(builder =>
                {
                });

            var builder = new MiddlewareBuilder<string>()
                //.Use<MyMiddleware>()
                //.Use(async (message, next) =>
                //{
                //    _testOutputHelper.WriteLine("Before 1");
                //    await next();
                //    _testOutputHelper.WriteLine("After 1");
                //})
                //.Use(async (message, next) =>
                //{
                //    _testOutputHelper.WriteLine("Before 2");
                //    await next();
                //    _testOutputHelper.WriteLine("After 2");
                //})
                .Batch()
                //.Use<MyBatchMiddleware>()
                //.Use(async (messages, next) =>
                //{
                //    _testOutputHelper.WriteLine("Before Batch 1");
                //    await next();
                //    _testOutputHelper.WriteLine("After Batch 1");
                //})
                ;

            var serviceProvider = host.Build().Services;
            var pipline = builder.Build();

            await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ =>
            {

                return pipline.Invoke(new Message<string>(), CancellationToken.None, new MessageContext(serviceProvider));
            }));
        }

        private class MyMiddleware : IMiddleware<string>
        {
            private readonly ILogger<MyMiddleware> _logger;

            public MyMiddleware(ILogger<MyMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Invoke(Message<string> message, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Single<string> next)
            {
                _logger.LogInformation($"Before {nameof(MyMiddleware)}");

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

            public async Task Invoke(IReadOnlyCollection<Message<string>> messages, CancellationToken cancellationToken, MessageContext messageContext, MessageDelegate.Batch<string> next)
            {
                _logger.LogInformation($"Before {nameof(MyBatchMiddleware)}");

                await next(messages, cancellationToken, messageContext);

                _logger.LogInformation($"After {nameof(MyBatchMiddleware)}");
            }
        }
    }
}

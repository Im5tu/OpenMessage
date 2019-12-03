using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Pipelines.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.MediatR.Tests
{
    public class MediatRTests : IDisposable, IAsyncLifetime
    {
        private readonly IList<string> _history = new List<string>();
        private readonly IHostBuilder _hostBuilder;
        private IHost _app;

        public MediatRTests(ITestOutputHelper testOutputHelper)
        {
            _hostBuilder = Host.CreateDefaultBuilder()
                               .ConfigureServices(services =>
                               {
                                   services.AddMediatR(typeof(MediatRTests).Assembly)
                                           .AddSingleton(_ => _history);
                               })
                               .ConfigureMessaging(builder =>
                               {
                                   builder.ConfigureMemory<string>()
                                          .Build();

                                   builder.ConfigurePipeline<string>()
                                          .UseDefaultMiddleware()
                                          .Use(async (message, next) =>
                                          {
                                              _history.Add("Middleware");
                                              await next();
                                              _history.Add("Middleware");
                                          })
                                          .Batch()
                                          .RunMediatR();
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
        public async Task MediatRHandlersAreCalled()
        {
            _app = _hostBuilder.Build();

            await _app.StartAsync();

            await _app.Services.GetRequiredService<IDispatcher<string>>()
                      .DispatchAsync("");

            var i = 0;
            Assert.Equal("Middleware", _history.ElementAtOrDefault(i++));
            Assert.Equal(nameof(BatchHandler), _history.ElementAtOrDefault(i++));
            Assert.Equal(nameof(Handler), _history.ElementAtOrDefault(i++));
            Assert.Equal("Middleware", _history.ElementAtOrDefault(i++));
        }

        private class Handler : INotificationHandler<MediatRMessage<string>>
        {
            private readonly IList<string> _history;

            public Handler(IList<string> history) => _history = history;

            public Task Handle(MediatRMessage<string> notification, CancellationToken cancellationToken)
            {
                _history.Add(nameof(Handler));

                return Task.CompletedTask;
            }
        }

        private class BatchHandler : INotificationHandler<MediatRBatch<string>>
        {
            private readonly IList<string> _history;

            public BatchHandler(IList<string> history) => _history = history;

            public Task Handle(MediatRBatch<string> notification, CancellationToken cancellationToken)
            {
                _history.Add(nameof(BatchHandler));

                return Task.CompletedTask;
            }
        }
    }
}
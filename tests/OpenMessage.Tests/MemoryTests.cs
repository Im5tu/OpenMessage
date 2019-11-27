using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Pipelines;
using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;
using OpenMessage.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests
{
    public class MemoryTests : IDisposable
    {
        private IHost _app;
        private bool _finished;
        private readonly IHostBuilder _hostBuilder;
        private bool _fireAndForget;

        public MemoryTests(ITestOutputHelper testOutputHelper)
        {
            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddLogging())
                .ConfigureLogging(builder => builder.AddTestOutputHelper(testOutputHelper))
                .ConfigureMessaging(builder =>
                {
                    builder
                        .ConfigureMemory<string>()
                        .ConfigureOptions(options => options.DispatcherFireAndForget = _fireAndForget)
                        .Build();

                    builder.ConfigurePipeline<string>(options =>
                        {
                            options.PipelineType = PipelineType.Serial;
                        })
                        .Use<AutoAcknowledgeMiddleware<string>>()
                        .Run(async message =>
                        {
                            await Task.Delay(1000);

                            _finished = true;
                        });
                });

        }

        [Fact]
        public async Task GivenFireAndForgetIsOff_WhenPipelineIsSerialAndAutoAcknowledgementIsEnabled_ThenAwaitingTheDispatchOfTheEventWillWaitForTheConsumerToFinish()
        {
            _fireAndForget = false;
            _app = _hostBuilder.Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            Assert.True(_finished);
        }

        [Fact]
        public async Task GivenFireAndForgetIsOn_WhenPipelineIsSerialAndAutoAcknowledgementIsEnabled_ThenAwaitingTheDispatchOfTheEventWillWaitForTheConsumerToFinish()
        {
            _fireAndForget = true;
            _app = _hostBuilder.Build();

            await _app.StartAsync();
            await _app.Services.GetRequiredService<IDispatcher<string>>().DispatchAsync("");

            Assert.False(_finished);
        }

        public void Dispose()
        {
            _app?.Dispose();
        }
    }
}
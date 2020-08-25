using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OpenMessage.Tests.Helpers
{
    internal static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddTestOutputHelper(this ILoggingBuilder builder, ITestOutputHelper testOutputHelper)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new TestOutputHelperLoggerProvider(testOutputHelper));

            return builder;
        }
    }
}
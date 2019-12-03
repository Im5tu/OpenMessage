using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace OpenMessage.Tests.Helpers
{
    internal sealed class TestOutputHelperLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLoggerProvider(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        public ILogger CreateLogger(string categoryName) => new TestOutputHelperLogger(_testOutputHelper);

        public void Dispose() { }

        private class TestOutputHelperLogger : ILogger
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public TestOutputHelperLogger(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

            public IDisposable BeginScope<TState>(TState state)
            {
                _testOutputHelper.WriteLine($"Begin Scope: {state}");

                return new ActionDisposable(() => _testOutputHelper.WriteLine($"End Scope: {state}"));
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _testOutputHelper.WriteLine($"Level: {logLevel}, EventId: {eventId}, Message: {formatter(state, exception)}");
            }
        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _action;

            public ActionDisposable(Action action) => _action = action;

            public void Dispose()
            {
                _action();
            }
        }
    }
}
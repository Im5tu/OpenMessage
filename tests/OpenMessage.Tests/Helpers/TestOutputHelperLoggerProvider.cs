using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OpenMessage.Tests.Helpers
{
    internal class TestOutputHelperLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Dispose()
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputHelperLogger(_testOutputHelper);
        }

        private class TestOutputHelperLogger : ILogger
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public TestOutputHelperLogger(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _testOutputHelper.WriteLine($"Level: {logLevel}, EventId: {eventId}, Message: {formatter(state, exception)}");
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                _testOutputHelper.WriteLine($"Begin Scope: {state}");

                return new ActionDisposable(() => _testOutputHelper.WriteLine($"End Scope: {state}"));
            }
        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _action;

            public ActionDisposable(Action action)
            {
                _action = action;
            }

            public void Dispose() => _action();
        }
    }
}
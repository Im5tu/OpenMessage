using FluentAssertions;
using System;
using Xunit;

namespace OpenMessage.Tests
{
    public class DispatchExtensionsTests
    {
        public class DispatchAsync
        {
            [Fact]
            public void GivenANullDispatcherThrowArgumentNullException()
            {
                Action act = () => DispatcherExtensions.DispatchAsync(default(IDispatcher<string>), "test");

                act.ShouldThrow<ArgumentNullException>();
            }
        }
    }
}

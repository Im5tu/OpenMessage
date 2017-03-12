using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenMessage.Providers.Memory.Tests
{
    public class MemoryChannelTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenANullLoggerThrowArgumentNullException()
            {
                Action act = () => new MemoryChannel<string>(null, Enumerable.Empty<IDispatchInterceptor<string>>());

                act.ShouldThrow<ArgumentNullException>().And.ParamName.Equals("logger");
            }

            [Fact]
            public void GivenANullEnumerableOfDispatchInterceptorsThrowArgumentNullException()
            {
                Action act = () => new MemoryChannel<string>(new Mock<ILogger<ManagedObservable<string>>>().Object, null);

                act.ShouldThrow<ArgumentNullException>().And.ParamName.Equals("dispatchInterceptors");
            }
        }

        public class DispatchAsync
        {
            [Fact]
            public void GivenANullEntityThrowArgumentNullException()
            {
                var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());

                Action act = () => target.DispatchAsync(null);
                act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("entity");
            }

            [Fact]
            public void GivenANullEntityWhenSuppliedWithAScheduleThrowArgumentNullException()
            {
                var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());

                Action act = () => target.DispatchAsync(null, TimeSpan.Zero);
                act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("entity");
            }

            [Fact]
            public void GivenAnInvalidScheduleTimeThrowArgumentException()
            {
                var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());

                Action act = () => target.DispatchAsync(string.Empty, TimeSpan.MinValue);
                act.ShouldThrow<ArgumentException>();
            }

            [Fact]
            public void GivenAnEntityWhenInterceptorReturnsFalseThenTaskFaultsWithException()
            {
                var interceptor = new Mock<IDispatchInterceptor<string>>();
                interceptor.Setup(x => x.Intercept(It.IsAny<string>())).Returns(false);
                var target = new MemoryChannel<string>(DefaultLogger(), new[] { interceptor.Object });

                var tsk = target.DispatchAsync("test");
                try
                {
                    tsk.Wait();
                }
                catch { }

                tsk.IsFaulted.Should().BeTrue();
                tsk.Exception.Should().NotBeNull();
                interceptor.Verify(x => x.Intercept(It.IsAny<string>()), Times.Once);
            }
        }

        public class Subscribe
        {
            [Fact]
            public void GivenANullObserverThrowArgumentNullException()
            {
                var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());

                Action act = () => target.Subscribe(null);

                act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("observer");
            }

            [Fact]
            public void GivenAnObserverThenShouldReturnNonNullObserver()
            {
                var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());

                target.Subscribe(new ActionObserver<string>(str => { })).Should().NotBeNull();
            }
        }

        [Fact]
        public void GivenAnEntityIsDispatchedThenRegisteredObserverReceivesMessage()
        {
            var testObject = Guid.NewGuid().ToString();
            var target = new MemoryChannel<string>(DefaultLogger(), DefaultEnumerable());
            var called = false;
            using (target.Subscribe(new ActionObserver<string>(str =>
            {
                if (str == testObject)
                    called = true;
            })))
                target.DispatchAsync(testObject).Wait();

            called.Should().BeTrue();
        }

        static ILogger<ManagedObservable<string>> DefaultLogger() => new Mock<ILogger<ManagedObservable<string>>>().Object;
        static IEnumerable<IDispatchInterceptor<string>> DefaultEnumerable() => Enumerable.Empty<IDispatchInterceptor<string>>();
    }
}

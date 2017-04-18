using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMessage.Providers.TestSpecifications;
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

        public class DispatchAsync : DispatcherTests<string>
        {
            protected override string InterceptEntity { get; } = "The registered interceptor will catch this entity.";

            protected override IServiceCollection ConfigureServices(IServiceCollection services) => 
                base.ConfigureServices(services).AddMemoryChannel<string>();
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

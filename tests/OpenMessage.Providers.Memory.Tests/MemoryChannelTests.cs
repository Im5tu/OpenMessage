using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMessage.Providers.TestSpecifications;
using System;
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

        public class Subscribe : ObservableTests<string>
        {
            protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
                services.AddMemoryChannel<string>();
        }

        [Fact]
        public void GivenAnEntityIsDispatchedThenRegisteredObserverReceivesMessage()
        {
            var testObject = Guid.NewGuid().ToString();
            var target = new MemoryChannel<string>(new Mock<ILogger<ManagedObservable<string>>>().Object, Enumerable.Empty<IDispatchInterceptor<string>>());
            var called = false;
            using (target.Subscribe(new ActionObserver<string>(str =>
            {
                if (str == testObject)
                    called = true;
            })))
                target.DispatchAsync(testObject).Wait();

            called.Should().BeTrue();
        }
    }
}

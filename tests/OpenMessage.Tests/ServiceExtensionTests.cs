using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace OpenMessage.Tests
{
    public class ServiceExtensionTests
    {
        public class AddObserver
        {
            [Fact]
            public void GivenANullServiceCollectionThrowArgumentNullException()
            {
                Action act = () => ServiceExtensions.AddObserver<string>(null, str => { });

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenANullActionThrowArgumentNullException()
            {
                Action act = () => new ServiceCollection().AddObserver<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenAnActionThenRegistersObserverInServiceCollection()
            {
                var services = new ServiceCollection().AddObserver<string>(str => { });

                services.Any(x => x.ServiceType == typeof(IObserver<string>)).Should().BeTrue();
            }
        }

        public class AddBroker
        {
            [Fact]
            public void GivenANullServiceCollectionThrowArgumentNullException()
            {
                Action act = () => ServiceExtensions.AddBroker<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenASingleInvocationThenAddsBrokerHost()
            {
                var services = new ServiceCollection().AddBroker<string>();

                services.Any(x => x.ServiceType == typeof(IBrokerHost)).Should().BeTrue();
            }

            [Fact]
            public void GivenMultipleInvocationsThenAddsSingleBrokerHost()
            {
                var services = new ServiceCollection().AddBroker<string>().AddBroker<string>();

                services.Count(x => x.ServiceType == typeof(IBrokerHost)).Should().Be(1);
            }

            [Fact]
            public void GivenASingleInvocationThenAddsBroker()
            {
                var services = new ServiceCollection().AddBroker<string>();

                services.Any(x => x.ServiceType == typeof(IBroker)).Should().BeTrue();
            }

            [Fact]
            public void GivenMultipleInvocationsThenAddsSingleBroker()
            {
                var services = new ServiceCollection().AddBroker<string>().AddBroker<string>();

                services.Count(x => x.ServiceType == typeof(IBroker)).Should().Be(1);
            }
        }
    }
}

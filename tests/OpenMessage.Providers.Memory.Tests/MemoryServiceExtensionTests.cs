using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace OpenMessage.Providers.Memory.Tests
{
    public class MemoryServiceExtensionTests
    {
        public class AddMemoryChannel
        {
            [Fact]
            public void GivenANullServiceCollectionThrowArgumentNullException()
            {
                Action act = () => MemoryServiceExtensions.AddMemoryChannel<string>(default(IServiceCollection));

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenAServiceCollectionThenAddsDispatcher()
            {
                var services = new ServiceCollection().AddMemoryChannel<string>();

                services.Any(service => service.ServiceType == typeof(IDispatcher<string>)).Should().BeTrue();
            }

            [Fact]
            public void GivenAServiceCollectionThenAddsObservable()
            {
                var services = new ServiceCollection().AddMemoryChannel<string>();

                services.Any(service => service.ServiceType == typeof(IObservable<string>)).Should().BeTrue();
            }
        }
    }
}

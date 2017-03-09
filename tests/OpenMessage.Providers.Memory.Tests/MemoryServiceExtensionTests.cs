using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        }

    }
}

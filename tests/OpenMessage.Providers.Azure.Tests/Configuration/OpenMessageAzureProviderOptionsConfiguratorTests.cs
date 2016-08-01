using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using OpenMessage.Providers.Azure.Configuration;
using System;
using Xunit;

namespace OpenMessage.Providers.Azure.Tests.Configuration
{
    public class OpenMessageAzureProviderOptionsConfiguratorTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenNullDefaultOptionsThrowArgumentNullException()
            {
                Action act = () => new OpenMessageAzureProviderOptionsConfigurator<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        public class ConfigureOptions
        {
            [Fact]
            public void GivenAOptionSetOverridesDefaultValues()
            {
                var defaultOptions = new OpenMessageAzureProviderOptions<string>
                {
                    ConnectionString = "default"
                };
                var mockOptions = new Mock<IOptions<OpenMessageAzureProviderOptions<string>>>();
                mockOptions.Setup(x => x.Value).Returns(defaultOptions);
                var target = new OpenMessageAzureProviderOptionsConfigurator<string>(mockOptions.Object);

                var testOptions = new OpenMessageAzureProviderOptions<string>();
                target.Configure(testOptions);

                testOptions.ConnectionString.Should().Be(defaultOptions.ConnectionString);
            }
        }
    }
}

using Microsoft.Extensions.Options;
using System;

namespace OpenMessage.Providers.Azure.Configuration
{
    internal sealed class OpenMessageAzureProviderOptionsConfigurator<T> : IConfigureOptions<OpenMessageAzureProviderOptions<T>>
    {
        private readonly OpenMessageAzureProviderOptions _defaultOptions;

        public OpenMessageAzureProviderOptionsConfigurator(IOptions<OpenMessageAzureProviderOptions> defaultOptions)
        {
            if (defaultOptions == null)
                throw new ArgumentNullException(nameof(defaultOptions));

            _defaultOptions = defaultOptions.Value;
        }

        public void Configure(OpenMessageAzureProviderOptions<T> options)
        {
            options.ConnectionString = _defaultOptions.ConnectionString;
        }
    }
}

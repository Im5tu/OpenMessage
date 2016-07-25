using Microsoft.Extensions.DependencyInjection;
using System;

namespace OpenMessage.Serializer.ProtobufNet
{
    public static class ServiceExtensions
    {
        /// <summary>
        ///     Adds a protobuf-net serializer to OpenMessage.
        /// </summary>
        public static IServiceCollection AddProtobufNetSerializer(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient<ISerializer, ProtobufNetSerializer>();
        }
    }
}

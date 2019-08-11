using OpenMessage.Samples.Core.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        public static IServiceCollection AddProducerService<T>(this IServiceCollection services)
            where T : class, new()
        {
            return services.AddHostedService<ProducerService<T>>();
        }

        public static IServiceCollection AddMassProducerService<T>(this IServiceCollection services)
            where T : class, new()
        {
            return services.AddHostedService<MassProducerService<T>>();
        }
    }
}
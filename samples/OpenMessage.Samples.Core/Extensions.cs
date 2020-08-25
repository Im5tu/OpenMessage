using OpenMessage.Samples.Core.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extensions
    {
        public static IServiceCollection AddSampleCore(this IServiceCollection services)
            => services.AddHostedService<DiagnosticService>();

        public static IServiceCollection AddMassProducerService<T>(this IServiceCollection services)
            where T : class, new()
            => services.AddHostedService<MassProducerService<T>>();

        public static IServiceCollection AddProducerService<T>(this IServiceCollection services)
            where T : class, new()
            => services.AddHostedService<ProducerService<T>>();
    }
}
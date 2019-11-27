using Microsoft.Extensions.DependencyInjection;
using OpenMessage.Pipelines.Builders;

namespace OpenMessage.MediatR
{
    /// <summary>
    /// MediatR extensions for OpenMessage
    /// </summary>
    public static class MediatRExtensions
    {
        /// <summary>
        /// Adds the required services to the collection
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMessagingBuilder AddMediatR(this IMessagingBuilder builder)
        {
            builder.Services.AddScoped(typeof(MediatRPipelineEndpoint<>));
            builder.Services.AddScoped(typeof(MediatRBatchPipelineEndpoint<>));

            return builder;
        }

        /// <summary>
        /// Adds the MediatR pipeline endpoint to the end of the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipelineBuilder"></param>
        public static void RunMediatR<T>(this IPipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.Run<MediatRPipelineEndpoint<T>>();
        }

        /// <summary>
        /// Adds the MediatR pipeline endpoint to the end of the pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipelineBuilder"></param>
        public static void RunMediatR<T>(this IBatchPipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.Run<MediatRBatchPipelineEndpoint<T>>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenMessage.Pipelines.Builders;

namespace OpenMessage.MediatR
{
    /// <summary>
    ///     MediatR extensions for OpenMessage
    /// </summary>
    public static class MediatRExtensions
    {
        /// <summary>
        ///     Adds the MediatR pipeline endpoint to the end of the pipeline
        /// </summary>
        public static void RunMediatR<T>(this IPipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.Services.TryAddScoped<MediatRPipelineEndpoint<T>>();
            pipelineBuilder.Run<MediatRPipelineEndpoint<T>>();
        }

        /// <summary>
        ///     Adds the MediatR pipeline endpoint to the end of the pipeline
        /// </summary>
        public static void RunMediatR<T>(this IBatchPipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.Services.TryAddScoped<MediatRBatchPipelineEndpoint<T>>();
            pipelineBuilder.Run<MediatRBatchPipelineEndpoint<T>>();
        }
    }
}
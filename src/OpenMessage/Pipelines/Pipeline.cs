using OpenMessage.Pipelines.Builders;
using OpenMessage.Pipelines.Middleware;

namespace OpenMessage.Pipelines
{
    /// <summary>
    /// Default pipeline methods
    /// </summary>
    public static class Pipeline
    {
        /// <summary>
        /// The default builder used when one isn't configured
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IPipelineBuilder<T> CreateDefaultBuilder<T>()
        {
            return new PipelineBuilder<T>()
                .Use<AutoAcknowledgeMiddleware<T>>();
        }
    }
}

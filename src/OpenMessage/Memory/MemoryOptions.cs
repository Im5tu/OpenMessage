using OpenMessage.Pipelines;

namespace OpenMessage.Memory
{
    /// <summary>
    /// Options to configure the memory provider
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryOptions<T>
    {
        /// <summary>
        /// Defaults to true. When false, awaiting a <see cref="IDispatcher{T}"/> will wait for the message to be consumed by the consumer (note that <see cref="PipelineOptions{T}"/> PipelineType must also be set to <see cref="PipelineType.Serial"/>)
        /// </summary>
        public bool FireAndForget { get; set; } = true;
    }
}

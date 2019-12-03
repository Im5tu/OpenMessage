namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     The type of the pipeline
    /// </summary>
    public enum PipelineType
    {
        /// <summary>
        ///     Each message in the pipeline is handled sequentially. Batching will produces batches of 1.
        /// </summary>
        Serial,

        /// <summary>
        ///     Each message in the pipeline is handled in a new task
        /// </summary>
        Parallel
    }
}
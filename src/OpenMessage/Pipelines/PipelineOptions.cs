using System;

namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     The options for general configuration of pipelines
    /// </summary>
    /// <typeparam name="T">The type of message in the pipeline</typeparam>
    public class PipelineOptions<T>
    {
        /// <summary>
        ///     The time it takes before the cancellation token is triggered. Default: 5 seconds
        /// </summary>
        public TimeSpan PipelineTimeout { get; set; }

        /// <summary>
        ///     Determines the pipeline type to use. Default: Parallel.
        /// </summary>
        public PipelineType PipelineType { get; set; } = PipelineType.Parallel;

        /// <summary>
        ///     Automatically confirm the message when the message has this capability. Default: true
        /// </summary>
        public bool? AutoAcknowledge { get; set; }

        /// <summary>
        ///     Determines whether or not to use a bounded channel.
        /// </summary>
        public bool? UseBoundedChannel { get; set; }

        /// <summary>
        ///     The number of messages to allow in the bounded channel.
        /// </summary>
        public int? BoundedChannelLimit { get; set; }
    }
}
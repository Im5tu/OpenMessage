using System.Threading.Tasks;

namespace OpenMessage
{
    /// <summary>
    ///     Indicates that the message supports acknowledgement
    /// </summary>
    public interface ISupportAcknowledgement
    {
        /// <summary>
        ///     The current state of the message, in terms of acknowledgement
        /// </summary>
        AcknowledgementState AcknowledgementState { get; }

        /// <summary>
        ///     Acknowledges the message back to the message source.
        /// </summary>
        /// <param name="positivelyAcknowledge">Indicates whether or not to positively acknowledge the message, or negatively acknowledge</param>
        /// <returns>A task that completes when the message source has completed the acknowledgement</returns>
        Task AcknowledgeAsync(bool positivelyAcknowledge = true);
    }
}
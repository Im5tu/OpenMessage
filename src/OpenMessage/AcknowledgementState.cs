namespace OpenMessage
{
    /// <summary>
    ///     The current status of the message acknowledgement
    /// </summary>
    public enum AcknowledgementState
    {
        /// <summary>
        ///     The message has not been acknowledged
        /// </summary>
        NotAcknowledged = 0,

        /// <summary>
        ///     The message has been acknowledged
        /// </summary>
        Acknowledged = 1,

        /// <summary>
        ///     The message has been negatively acknowledged
        /// </summary>
        NegativelyAcknowledged = 2
    }
}
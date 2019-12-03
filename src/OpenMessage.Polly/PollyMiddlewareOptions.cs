namespace OpenMessage.Polly
{
    /// <summary>
    ///     The options for the Polly Middleware
    /// </summary>
    public class PollyMiddlewareOptions<T>
    {
        /// <summary>
        ///     The name of the policy to use
        /// </summary>
        public string PolicyName { get; set; }
    }
}
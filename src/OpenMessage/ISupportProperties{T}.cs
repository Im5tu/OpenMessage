namespace OpenMessage
{
    /// <summary>
    ///     Indicates that the message supports properties
    /// </summary>
    public interface ISupportProperties<TValue> : ISupportProperties<string, TValue>
    {
    }
}
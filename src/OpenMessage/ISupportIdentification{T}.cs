using System.Diagnostics.CodeAnalysis;

namespace OpenMessage
{
    /// <summary>
    ///     Indicates the message supports identification
    /// </summary>
    /// <typeparam name="T">The type of the message identifier</typeparam>
    public interface ISupportIdentification<T>
    {
        /// <summary>
        ///     The message id
        /// </summary>
        [MaybeNull, AllowNull]
        T Id { get; }
    }
}
#nullable enable

namespace OpenMessage.Extensions
{
    /// <summary>
    ///     Represents an entity which must be protected with one or more conditions
    /// </summary>
    /// <typeparam name="T">The type of instance being guarded</typeparam>
    public readonly struct Guarded<T>
    {
        /// <summary>
        ///     The entity which is under guard
        /// </summary>
        public readonly T Value;

        /// <summary>
        ///     The name of the entity
        /// </summary>
        public readonly string? Name;

        /// <summary>
        ///     ctor
        /// </summary>
        public Guarded(T value, string? name = null)
        {
            Value = value;
            Name = name;
        }
    }
}
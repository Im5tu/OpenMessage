using System.Reflection;

namespace OpenMessage
{
    /// <summary>
    ///     Fast access to certain type properties
    /// </summary>
    public static class TypeCache<T>
    {
        private static readonly TypeInfo _type = typeof(T).GetTypeInfo();

        /// <summary>
        ///     The friendly name of the class, with expanded generics
        /// </summary>
        public static string FriendlyName { get; } = _type.GetFriendlyName();

        /// <summary>
        ///     True, if the type is a class
        /// </summary>
        public static bool IsReferenceType { get; } = _type.IsClass;

        /// <summary>
        ///     The name of the class
        /// </summary>
        public static string Name { get; } = _type.Name;
    }
}
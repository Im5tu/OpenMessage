using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenMessage
{
    public static class TypeCache
    {
        private static ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        public static bool TryGetType(string typeName, [NotNullWhen(true)] out Type? type)
        {
            if (_types.TryGetValue(typeName, out  type))
                return true;

            var originalTypeName = typeName;
            var splitIndex = typeName.IndexOf(",", StringComparison.Ordinal);
            if (splitIndex > 0)
            {
                var nextSplitIndex = typeName.IndexOf(",", Math.Min(typeName.Length, splitIndex + 1), StringComparison.Ordinal);
                typeName = nextSplitIndex == -1
                    ? typeName.Substring(0, splitIndex)
                    : typeName.Substring(0, nextSplitIndex);
            }

            type = Type.GetType(typeName);

            if (type != null)
                _types.TryAdd(originalTypeName, type);

            return type != null;
        }
    }

    /// <summary>
    ///     Fast access to certain type properties
    /// </summary>
    public static class TypeCache<T>
    {
        private static readonly TypeInfo _type = typeof(T).GetTypeInfo();

        /// <summary>
        ///     The friendly name of the class, with expanded generics
        /// </summary>
        public static string? FriendlyName = _type.GetFriendlyName();

        /// <summary>
        ///     True, if the type is a class
        /// </summary>
        public static bool IsReferenceType = _type.IsClass;

        /// <summary>
        ///     The assembly qualified name of the type
        /// </summary>
        public static string? AssemblyQualifiedName = _type.AssemblyQualifiedName;

        /// <summary>
        ///     True, if the type is abstract
        /// </summary>
        public static bool IsAbstract = _type.IsAbstract;

        /// <summary>
        ///     True, if the type is abstract or if the type is an interface
        /// </summary>
        public static bool IsAbstractOrInterface = _type.IsAbstract || _type.IsInterface;

        /// <summary>
        ///    True, if the type is an interface
        /// </summary>
        public static bool IsInterface = _type.IsInterface;

        /// <summary>
        ///     The name of the class
        /// </summary>
        public static string? Name = _type.Name;
    }
}
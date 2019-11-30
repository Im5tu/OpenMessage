using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OpenMessage
{
    internal static class TypeNames
    {
        private static readonly ConcurrentDictionary<Type, string> _friendlyNames = new ConcurrentDictionary<Type, string>();

        public static string GetFriendlyName(this Type type)
        {
            if (type is null)
                Throw.ArgumentNullException(nameof(type));

            return _friendlyNames.GetOrAdd(type, key =>
            {
                if (key.IsGenericType)
                    return $"{key.Namespace}.{key.Name.Remove(key.Name.IndexOf('`'))}<{string.Join(", ", key.GetGenericArguments().Select(GetFriendlyName))}>";

                return $"{key.Namespace}.{key.Name}";
            });
        }
    }
}
using System;
using System.Linq;

namespace OpenMessage.Providers.Azure
{
    internal static class TypeNameExtensions
    {
        internal static string GetFriendlyName(this Type type)
        {
            if (type.IsGenericType)
                return $"{type.Name.Remove(type.Name.IndexOf('`'))}<{string.Join(",", type.GetGenericArguments().Select(t => t.GetFriendlyName()))}>";

            return type.Name;
        }

        internal static string AsAzureSafeString(this string str)
        {
            var tempStr = str.Replace('<', '_').Replace('>', '_');

            if (tempStr.EndsWith("_"))
                return tempStr.Substring(0, tempStr.Length - 1);

            return tempStr;
        }
    }
}

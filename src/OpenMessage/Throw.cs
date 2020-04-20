using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenMessage
{
    /// <summary>
    ///     Helpers for throwing an exception to aid in the ability for smaller methods to inline
    /// </summary>
    public static class Throw
    {
        /// <summary>
        ///     Throws a ArgumentException
        /// </summary>
        /// <exception cref="System.ArgumentException">The exception</exception>
        [DoesNotReturn]
        public static void ArgumentException()
        {
            throw new ArgumentException();
        }

        /// <summary>
        ///     Throws a ArgumentException
        /// </summary>
        /// <param name="paramName">The parameter name to use</param>
        /// <param name="message">The message to to use in the exception</param>
        /// <exception cref="System.ArgumentException">The exception with a specified parameter name &amp; message</exception>
        [DoesNotReturn]
        public static void ArgumentException(string paramName, string message)
        {
            throw new ArgumentException(paramName, message);
        }

        /// <summary>
        ///     Throws a ArgumentNullException
        /// </summary>
        /// <exception cref="System.ArgumentNullException">The exception</exception>
        [DoesNotReturn]
        public static void ArgumentNullException()
        {
            throw new ArgumentNullException();
        }

        /// <summary>
        ///     Throws a ArgumentNullException
        /// </summary>
        /// <param name="paramName">The parameter name to use</param>
        /// <param name="message">The message to to use in the exception</param>
        /// <exception cref="System.ArgumentNullException">The exception with a specified message, if applicable</exception>
        [DoesNotReturn]
        public static void ArgumentNullException(string paramName, string message = "Parameter cannot be null.")
        {
            throw new ArgumentNullException(paramName, message);
        }

        /// <summary>
        ///     Throws a Exception
        /// </summary>
        /// <param name="message">The message to to use in the exception</param>
        /// <exception cref="System.Exception">The exception with a specified message, if applicable</exception>
        [DoesNotReturn]
        public static void Exception(string? message = null)
        {
            if (message is null)
                throw new Exception();

            throw new Exception(message);
        }
    }
}
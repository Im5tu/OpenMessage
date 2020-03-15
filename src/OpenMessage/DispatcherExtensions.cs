using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMessage
{
    /// <summary>
    ///     Extensions for the dispatcher
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity) => dispatcher.DispatchAsync(entity, default);

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <param name="attributes">The attributes to send along with the message</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            var message = new ExtendedMessage<T> { Value = entity, Properties = attributes ?? Enumerable.Empty<KeyValuePair<string, string>>() };
            return dispatcher.DispatchAsync(message, default);
        }

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <param name="attributes">The attributes to send along with the message</param>
        /// <param name="id">The id to use for the message</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity, IEnumerable<KeyValuePair<string, string>> attributes, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                Throw.ArgumentNullException(nameof(id));

            var message = new ExtendedMessage<T> { Value = entity, Properties = attributes ?? Enumerable.Empty<KeyValuePair<string, string>>(), Id = id };
            return dispatcher.DispatchAsync(message, default);
        }

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <param name="attribute">The attributes to send along with the message</param>
        /// <param name="id">The id to use for the message</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity, KeyValuePair<string, string> attribute, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                Throw.ArgumentNullException(nameof(id));

            var message = new ExtendedMessage<T> { Value = entity, Properties = new [] { attribute }, Id = id };
            return dispatcher.DispatchAsync(message, default);
        }

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="entity">The entity to dispatch</param>
        /// <param name="id">The id to use for the message</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, T entity, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                Throw.ArgumentNullException(nameof(id));

            var message = new ExtendedMessage<T> { Value = entity, Id = id };
            return dispatcher.DispatchAsync(message, default);
        }

        /// <summary>
        ///     Dispatches the specified entity
        /// </summary>
        /// <param name="dispatcher">The dispatcher in use</param>
        /// <param name="message">The message to dispatch</param>
        /// <returns>A task that completes when the message has been acknowledged by the receiver</returns>
        public static Task DispatchAsync<T>(this IDispatcher<T> dispatcher, Message<T> message) => dispatcher.DispatchAsync(message, default);
    }
}
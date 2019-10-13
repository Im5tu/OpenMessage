using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace OpenMessage.Extensions
{
    /// <summary>
    ///     Helper methods for <see cref="Guarded{T}" />
    /// </summary>
    public static class GuardExtensions
    {
        /// <summary>
        ///     Places the specified entity under guard
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        /// <param name="name">The name of the entity under guard</param>
        /// <typeparam name="T">The type of the entity under guard</typeparam>
        /// <returns>An instance of <see cref="Guarded{T}" /></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guarded<T> Must<T>(this T entity, string? name = null)
        {
            return new Guarded<T>(entity, name);
        }

        /// <summary>
        ///     Enforce that the entity is not null.
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        /// <param name="message">The exception message to use.</param>
        /// <typeparam name="T">The type of the entity under guard</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNull<T>(this Guarded<T> entity, string? message = null)
        {
            if (!TypeCache<T>.IsReferenceType || entity.Value != null)
                return;

            switch (entity.Name)
            {
                case null:
                    Throw.ArgumentNullException();
                    break;
                default:
                    if (message == null)
                        Throw.ArgumentNullException(entity.Name);
                    else
                        Throw.ArgumentNullException(entity.Name, message);
                    break;
            }
        }

        /// <summary>
        ///     Enforce that the entity is not null or default.
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        /// <typeparam name="T">The type of the entity under guard</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrDefault<T>(this Guarded<T> entity)
        {
            #nullable disable
            if (!EqualityComparer<T>.Default.Equals(entity.Value, default(T)))
                return;
            #nullable restore

            if (entity.Name == null)
                Throw.ArgumentException();
            else
                Throw.ArgumentException(entity.Name, TypeCache<T>.IsReferenceType ? "Parameter cannot be null." : "Parameter cannot be default.");
        }

        /// <summary>
        ///     Enforce that the entity is not null or empty.
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty(this Guarded<string> entity)
        {
            if (!string.IsNullOrEmpty(entity.Value))
                return;

            if (entity.Name == null)
                Throw.ArgumentException();
            else
                Throw.ArgumentException(entity.Name, "Parameter cannot be null or empty.");
        }

        /// <summary>
        ///     Enforce that the entity is null and has elements in the array
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        /// <typeparam name="T">The type of the entity under guard</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty<T>(this Guarded<T[]> entity)
        {
            if (entity.Value?.Length > 0)
                return;

            if (entity.Name == null)
                Throw.ArgumentException();
            else
                Throw.ArgumentException(entity.Name, "Parameter cannot be null or empty.");
        }

        /// <summary>
        ///     Enforce that the entity is null and has elements in the collection
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        /// <typeparam name="T">The type of the entity under guard</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty<T>(this Guarded<ICollection<T>> entity)
        {
            if (entity.Value?.Count > 0)
                return;

            if (entity.Name == null)
                Throw.ArgumentException();
            else
                Throw.ArgumentException(entity.Name, "Parameter cannot be null or empty.");
        }

        /// <summary>
        ///     Enforce that the entity is not null, empty or whitespace.
        /// </summary>
        /// <param name="entity">The entity under guard</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrWhiteSpace(this Guarded<string> entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.Value))
                return;

            if (entity.Name == null)
                Throw.ArgumentException();
            else
                Throw.ArgumentException(entity.Name, "Parameter cannot be null, empty or whitespace.");
        }
    }
}
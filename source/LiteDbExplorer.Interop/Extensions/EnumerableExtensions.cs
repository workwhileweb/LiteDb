using System;
using System.Collections.Generic;

namespace LiteDbExplorer.Core
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns a <see cref="HashSet{T}"/> of the source items using the default equality
        /// comparer for the type.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in source sequence.</typeparam>
        /// <param name="source">Source sequence</param>
        /// <returns>A hash set of the items in the sequence, using the default equality comparer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
        /// <remarks>
        /// This evaluates the input sequence completely.
        /// </remarks>

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToHashSet(null);
        }

        /// <summary>
        /// Returns a <see cref="HashSet{T}"/> of the source items using the specified equality
        /// comparer for the type.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in source sequence.</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="comparer">Equality comparer to use; a value of null will cause the type's default equality comparer to be used</param>
        /// <returns>A hash set of the items in the sequence, using the default equality comparer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
        /// <remarks>
        /// This evaluates the input sequence completely.
        /// </remarks>

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new HashSet<TSource>(source, comparer);
        }
    }
}
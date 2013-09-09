using System.Collections.Generic;
using System.Linq;

namespace C5.intervals
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Check if an enumerable is null or empty
        /// </summary>
        /// <param name="enumerable">An enumerable</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if collection is either null or empty, otherwise false</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}

namespace hotelier_core_app.Domain.Extensions
{
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Adds a range of items to the specified collection.
        /// </summary>
        /// <typeparam name="TInput">The type of items in the collection.</typeparam>
        /// <param name="collection">The target collection to add items to.</param>
        /// <param name="addCollection">The collection of items to add.</param>
        public static void AddRange<TInput>(this ICollection<TInput> collection, IEnumerable<TInput> addCollection)
        {
            if (collection == null || addCollection == null)
            {
                return;
            }

            foreach (TInput item in addCollection)
            {
                collection.Add(item);
            }
        }
    }
}

using Microsoft.Azure.Cosmos;
using System.Collections.Generic;

namespace MenuApi.Extensions
{
    internal static class FeedIteratorExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this FeedIterator<T> iterator)
        {
            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }
    }
}

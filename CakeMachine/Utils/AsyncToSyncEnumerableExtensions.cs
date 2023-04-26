namespace CakeMachine.Utils;

internal static class AsyncToSyncEnumerableExtensions
{
    public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();

        await foreach(var element in asyncEnumerable)
            list.Add(element);

        return list;
    }

#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
#pragma warning restore CS1998
    {
        foreach (var element in enumerable)
            yield return element;
    }
}
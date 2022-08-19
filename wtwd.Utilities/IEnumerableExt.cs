namespace wtwd.Utilities;

public static class IEnumerableExt
{
    public static IEnumerable<(T, T?)> Lag<T>(this IEnumerable<T> collection, int lagSize = 1)
    {
        if (lagSize < 0)
            throw new ArgumentOutOfRangeException(nameof(lagSize), lagSize, "Non-negative integer expected");

        Queue<T> laggedElements = new Queue<T>(lagSize + 1);
        T? laggedResult = default(T);
        foreach (T element in collection)
        {
            laggedElements.Enqueue(element);
            if (!laggedElements.TryDequeue(out laggedResult))
                laggedResult = default(T);
            yield return (element, laggedResult);
        }
    }

    public static IEnumerable<(T, T?)> Lag<T>(this IEnumerable<T> collection, Func<T, T, bool> areElementsInTheSamePartition, int lagSize = 1)
    {
        throw new NotImplementedException();
    }
}

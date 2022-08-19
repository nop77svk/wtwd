namespace NoP77svk.wtwd.Utilities;

public static class IEnumerableExt
{
    public static IEnumerable<(T Current, T? Lagged)> Lag<T>(this IEnumerable<T> collection, int lagSize = 1)
    {
        return collection.Lag((_, _) => true, lagSize);
    }

    public static IEnumerable<(T Current, T? Lagged)> Lag<T>(this IEnumerable<T> collection, Func<T, T, bool> areElementsInTheSamePartition, int lagSize = 1)
    {
        if (lagSize < 0)
            throw new ArgumentOutOfRangeException(nameof(lagSize), lagSize, "Non-negative integer expected");

        Queue<T> laggedElements = new Queue<T>(lagSize);
        T? previousElement = default(T);
        foreach (T element in collection)
        {
            if (previousElement == null || !areElementsInTheSamePartition(element, previousElement))
                laggedElements.Clear();

            T? laggedResult;
            if (laggedElements.Count == lagSize)
                laggedResult = laggedElements.Dequeue();
            else
                laggedResult = default(T);

            yield return (element, laggedResult);

            laggedElements.Enqueue(element);
            previousElement = element;
        }
    }
}

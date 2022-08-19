namespace NoP77svk.wtwd.Utilities;

public static class IEnumerableExt
{
    public static IEnumerable<(T Current, T? Lagged)> Lag<T>(this IEnumerable<T> collection, int lagSize = 1)
    {
        return collection.Lag(_ => 0, lagSize);
    }

    public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement, TPartitionKey>(this IEnumerable<TElement> collection, Func<TElement, TPartitionKey> getPartitionKey, int lagSize = 1)
        where TPartitionKey : IEquatable<TPartitionKey>
    {
        if (lagSize < 0)
            throw new ArgumentOutOfRangeException(nameof(lagSize), lagSize, "Non-negative integer expected");

        Queue<TElement> laggedElements = new Queue<TElement>(lagSize);
        TElement? previousElement = default;
        foreach (TElement element in collection)
        {
            if (previousElement == null)
                laggedElements.Clear();
            else if (!getPartitionKey(element).Equals(getPartitionKey(previousElement)))
                laggedElements.Clear();

            TElement? resultLaggedElement;
            if (laggedElements.Count == lagSize)
                resultLaggedElement = laggedElements.Dequeue();
            else
                resultLaggedElement = default;

            yield return (element, resultLaggedElement);

            laggedElements.Enqueue(element);
            previousElement = element;
        }
    }
}

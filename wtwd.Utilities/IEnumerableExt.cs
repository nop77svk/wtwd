namespace NoP77svk.wtwd.Utilities;

public static class IEnumerableExt
{
    public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement>(this IEnumerable<TElement> collection, Func<TElement, TElement, bool> areInTheSamePartition, int lagSize = 1)
    {
        if (lagSize < 0)
            throw new ArgumentOutOfRangeException(nameof(lagSize), lagSize, "Non-negative integer expected");

        Queue<TElement> laggedElements = new Queue<TElement>(lagSize);
        TElement? previousElement = default;
        foreach (TElement element in collection)
        {
            if (previousElement == null)
                laggedElements.Clear();
            else if (!areInTheSamePartition(element, previousElement))
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

    public static IEnumerable<(T Current, T? Lagged)> Lag<T>(this IEnumerable<T> collection, int lagSize = 1)
    {
        return collection.Lag((_, _) => true, lagSize);
    }

    public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement, TPartitionKey>(this IEnumerable<TElement> collection, Func<TElement, TPartitionKey> getPartitionKey, int lagSize = 1)
        where TPartitionKey : IEquatable<TPartitionKey>
    {
        return collection.Lag((element, previousElement) => getPartitionKey(element).Equals(getPartitionKey(previousElement)));
    }

    public static IEnumerable<(int RunId, TElement Element)> RecognizeElementRuns<TElement>(this IEnumerable<TElement> collection, Func<TElement, TElement?, bool> areInTheSameRun)
    {
        int runId = 0;
        foreach ((TElement Current, TElement? Lagged) elementTuple in collection.Lag())
        {
            bool newRunStartsHere = runId == 0
                || !areInTheSameRun(elementTuple.Current, elementTuple.Lagged);

            if (newRunStartsHere)
                runId++;

            yield return (runId, elementTuple.Current);
        }
    }
}

namespace NoP77svk.Linq
{
    using System;
    using System.Collections.Generic;

    // 2do! add unit tests for this class
    public static class EnumerableAnalyticsExt
    {
        public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement>(this IEnumerable<TElement> collection, Func<TElement, TElement, bool> areInTheSamePartition, int lagSize = 1)
            where TElement : class
        {
            if (lagSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lagSize), lagSize, "Non-negative integer expected");
            }

            return LagInternal(collection, areInTheSamePartition, lagSize);
        }

        public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement>(this IEnumerable<TElement> collection, int lagSize = 1)
            where TElement : class
            => collection.Lag((current, lagged) => true, lagSize);

        public static IEnumerable<(TElement Current, TElement? Lagged)> Lag<TElement, TPartitionKey>(this IEnumerable<TElement> collection, Func<TElement, TPartitionKey> getPartitionKey, int lagSize = 1)
            where TElement : class
            where TPartitionKey : IEquatable<TPartitionKey>
            => collection.Lag((element, previousElement) => getPartitionKey(element).Equals(getPartitionKey(previousElement)));

        public static IEnumerable<(TElement Current, TElement? Leading)> Lead<TElement>(this IEnumerable<TElement> collection, Func<TElement, TElement, bool> areInTheSamePartition, int leadSize = 1)
            where TElement : class
        {
            if (leadSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(leadSize), leadSize, "Non-negative integer expected");
            }

            return LeadInternal(collection, areInTheSamePartition, leadSize);
        }

        public static IEnumerable<(TElement Current, TElement? Leading)> Lead<TElement>(this IEnumerable<TElement> collection, int leadSize = 1)
            where TElement : class
            => collection.Lead((current, lagged) => true, leadSize);

        public static IEnumerable<(TElement Current, TElement? Leading)> Lead<TElement, TPartitionKey>(this IEnumerable<TElement> collection, Func<TElement, TPartitionKey> getPartitionKey, int leadSize = 1)
            where TElement : class
            where TPartitionKey : IEquatable<TPartitionKey>
            => collection.Lead((element, previousElement) => getPartitionKey(element).Equals(getPartitionKey(previousElement)));

        public static IEnumerable<(int RunId, TElement Element)> RecognizeElementRuns<TElement>(this IEnumerable<TElement> collection, Func<TElement, TElement?, bool> areInTheSameRun)
            where TElement : class
        {
            int runId = 0;
            foreach ((TElement Current, TElement? Lagged) elementTuple in collection.Lag())
            {
                bool newRunStartsHere = runId == 0
                    || !areInTheSameRun(elementTuple.Current, elementTuple.Lagged);

                if (newRunStartsHere)
                {
                    runId++;
                }

                yield return (runId, elementTuple.Current);
            }
        }

        private static IEnumerable<(TElement Current, TElement? Lagged)> LagInternal<TElement>(IEnumerable<TElement> collection, Func<TElement, TElement, bool> areInTheSamePartition, int lagSize)
            where TElement : class
        {
            Queue<TElement> laggedElements = new Queue<TElement>(lagSize);
            TElement? previousElement = null;
            foreach (TElement element in collection)
            {
                if (previousElement != null && !areInTheSamePartition(element, previousElement))
                {
                    laggedElements.Clear();
                }

                TElement? resultLaggedElement;
                if (laggedElements.Count == lagSize)
                {
                    resultLaggedElement = laggedElements.Dequeue();
                }
                else
                {
                    resultLaggedElement = null;
                }

                yield return (element, resultLaggedElement);

                laggedElements.Enqueue(element);
                previousElement = element;
            }
        }

        private static IEnumerable<(TElement Current, TElement? Leading)> LeadInternal<TElement>(IEnumerable<TElement> collection, Func<TElement, TElement, bool> areInTheSamePartition, int leadSize)
            where TElement : class
        {
            Queue<TElement> laggedElements = new Queue<TElement>(leadSize);
            TElement? previousElement = null;

            foreach (TElement element in collection)
            {
                if (previousElement != null && !areInTheSamePartition(element, previousElement))
                {
                    while (laggedElements.Count > 0)
                    {
                        TElement laggedElement = laggedElements.Dequeue();
                        yield return (laggedElement, null);
                    }

                    laggedElements.Clear();
                }

                laggedElements.Enqueue(element);
                if (laggedElements.Count > leadSize)
                {
                    TElement laggedElement = laggedElements.Dequeue();
                    yield return (laggedElement, element);
                }

                previousElement = element;
            }

            while (laggedElements.Count > 0)
            {
                TElement? laggedElement = laggedElements.Dequeue();
                yield return (laggedElement, null);
            }
        }
    }
}

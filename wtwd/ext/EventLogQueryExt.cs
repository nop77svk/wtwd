#pragma warning disable CA1416
namespace wtwd.ext;

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

internal static class EventLogQueryExt
{
    internal static IEnumerable<EventRecord> AsEnumerable(this EventLogQuery query)
    {
        using EventLogReader reader = new EventLogReader(query);
        while (true)
        {
            using EventRecord? row = reader.ReadEvent();
            if (row == null) break;
            yield return row;
        }
    }
}

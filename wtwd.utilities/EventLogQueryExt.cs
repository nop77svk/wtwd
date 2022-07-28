#pragma warning disable CA1416
namespace wtwd.Utilities;

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

public static class EventLogQueryExt
{
    public static IEnumerable<EventRecord> AsEnumerable(this EventLogQuery query)
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

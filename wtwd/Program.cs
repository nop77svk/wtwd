#pragma warning disable CA1416
namespace wtwd;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Xml.Linq;

internal class Program
{
    internal static void Main(string[] args)
    {
        IEnumerable<PcStateChange> allEvents = ReadEventLogForPcStateChanges(DateTime.Now.AddMonths(-1));

        foreach (var row in allEvents.StateChangesToSessions())
        {
            if (row.IsStillRunning)
                Console.WriteLine($"{row.SessionFirstStart.EventAsString} @ {row.SessionFirstStart.When} -> (ongoing session)");
            else
                Console.WriteLine($"{row.SessionFirstStart.EventAsString} @ {row.SessionFirstStart.When} -> {row.SessionLastEnd?.EventAsString ?? "?"} @ {row.SessionLastEnd?.When.ToString() ?? "?"} = {row.FullSessionSpan?.ToString() ?? "?"}");
        }
    }

    private static IEnumerable<PcStateChange> ReadEventLogForPcStateChanges(DateTime since)
    {
        string sinceAsStr = since.ToUniversalTime().ToString("O");

        IEnumerable<EventRecord> unionedEvents = new List<EventRecord>();
        EventLogQuery? queryKernelBoot = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and (EventID = 20 or EventID = 25 or EventID = 27) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelBoot != null)
            unionedEvents = unionedEvents.Concat(queryKernelBoot.AsEnumerable());

        EventLogQuery? queryKernelGeneral = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-General' and (EventID = 12 or EventID = 13) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelGeneral != null)
            unionedEvents = unionedEvents.Concat(queryKernelGeneral.AsEnumerable());

        EventLogQuery? queryKernelPower = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Power' and (EventID = 109 or EventID = 42 or EventID = 107 or Event = 506 or Event = 507) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelPower != null)
            unionedEvents = unionedEvents.Concat(queryKernelPower.AsEnumerable());

        EventLogQuery? querySynTpEnhServiceForLockUnlock = new EventLogQuery("Application", PathType.LogName, @"Event[System[Provider/@Name = 'SynTPEnhService' and EventID = 0] and EventData/Data]");
        if (querySynTpEnhServiceForLockUnlock != null)
            unionedEvents = unionedEvents.Concat(querySynTpEnhServiceForLockUnlock.AsEnumerable());

        return unionedEvents
            .Where(evnt => evnt.TimeCreated != null)
            .Where(evnt => evnt.TimeCreated >= since)
            .Select(evnt => evnt.AsPcStateChange())
            .Where(stch => stch.How != PcStateChangeHow.Unknown && stch.What != PcStateChangeWhat.Unknown);
    }
}
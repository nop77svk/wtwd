#pragma warning disable CA1416
namespace wtwd;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Xml.Linq;
using wtwd.ext;
using wtwd.model;

internal class Program
{
    internal static void Main(string[] args)
    {
        DateTime logsSince = DateTime.Now.AddMonths(-1);
        TimeSpan roundingInterval = TimeSpan.FromMinutes(1);

        IEnumerable<PcSession> pcSessions = GetEventLogsSince(logsSince)
            .Select(evnt => evnt.AsPcStateChange())
            .Select(stch => stch with { When = stch.When.Round(roundingInterval)})
            .Where(stch => stch.How != PcStateChangeHow.Unknown && stch.What != PcStateChangeWhat.Unknown)
            .StateChangesToSessions()
            .Where(session => session.FullSessionSpan != TimeSpan.Zero);

        DisplayTheSessions(pcSessions);
    }

    private static void DisplayTheSessions(IEnumerable<PcSession> sessions)
    {
        foreach (var session in sessions)
        {
            if (session.IsStillRunning)
                Console.WriteLine($"[{session.SessionFirstStart.When.ToString("yyyy-MM-dd HH:mm")}] -> (ongoing session from {session.SessionFirstStart.EventAsString})");
            else
                Console.WriteLine($"[{session.SessionFirstStart.When.ToString("yyyy-MM-dd HH:mm")}] -> [{session.SessionLastEnd?.When.ToString("yyyy-MM-dd HH:mm") ?? "?"}] = {session.ShortSessionSpan?.ToString() ?? "?"} (full {session.FullSessionSpan?.ToString() ?? "?"} @ {session.SessionFirstStart.EventAsString} -> {session.SessionLastEnd?.EventAsString ?? "?"})");
        }
    }

    private static IEnumerable<EventRecord> GetEventLogsSince(DateTime since)
    {
        string sinceAsStr = since.ToUniversalTime().ToString("O");

        IEnumerable<EventRecord> unionedEvents = new List<EventRecord>();
        EventLogQuery? queryKernelBoot = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and (EventID = 20 or EventID = 25 or EventID = 27) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelBoot != null)
            unionedEvents = unionedEvents.Concat(queryKernelBoot.AsEnumerable());

        EventLogQuery? queryKernelGeneral = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-General' and (EventID = 12 or EventID = 13) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelGeneral != null)
            unionedEvents = unionedEvents.Concat(queryKernelGeneral.AsEnumerable());

        EventLogQuery? queryKernelPower = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Power' and (EventID = 109 or EventID = 42 or EventID = 107 or EventID = 506 or EventID = 507) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelPower != null)
            unionedEvents = unionedEvents.Concat(queryKernelPower.AsEnumerable());

        EventLogQuery? querySynTpEnhServiceForLockUnlock = new EventLogQuery("Application", PathType.LogName, @"Event[System[Provider/@Name = 'SynTPEnhService' and EventID = 0] and EventData/Data]");
        if (querySynTpEnhServiceForLockUnlock != null)
            unionedEvents = unionedEvents.Concat(querySynTpEnhServiceForLockUnlock.AsEnumerable()
                .Where(evnt => evnt.TimeCreated >= since)
            );

        return unionedEvents
            .Where(evnt => evnt.TimeCreated != null)
            .Where(evnt => evnt.TimeCreated >= since);
    }
}
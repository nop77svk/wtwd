#pragma warning disable CA1416
namespace wtwd;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using wtwd.ext;
using wtwd.model;

internal class Program
{
    private const string DayFormat = "yyyy-MM-dd";
    private const string NextDayFormat = "(yyyy-MM-dd) HH:mm";
    private const string TimeFormat = "HH:mm";
    private const string SessionSpanFormat = @"hh\:mm";

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
        PcSession? prevSession = null;
        foreach (var session in sessions)
        {
            if (prevSession == null || session.SessionFirstStart.When.Date != prevSession.SessionFirstStart.When.Date)
                System.Console.WriteLine($"{session.SessionFirstStart.When.ToString(DayFormat)}");

            if (session.SessionLastEnd == null || session.SessionFirstEnd == null)
                Console.WriteLine($"\t{session.SessionFirstStart.When.ToString(TimeFormat)} -> (ongoing session from {session.SessionFirstStart.EventAsString})");
            else if (session.SessionLastEnd.When.Date != session.SessionFirstStart.When.Date)
                Console.WriteLine($"\t{session.SessionFirstStart.When.ToString(TimeFormat)} -> {session.SessionLastEnd.When.ToString(NextDayFormat)} = [{session.ShortSessionSpan?.ToString(SessionSpanFormat) ?? "?"} / {session.FullSessionSpan?.ToString(SessionSpanFormat) ?? "?"}] {session.SessionFirstStart.EventAsString} -> {session.SessionLastEnd.EventAsString}");
            else
                Console.WriteLine($"\t{session.SessionFirstStart.When.ToString(TimeFormat)} -> {session.SessionLastEnd.When.ToString(TimeFormat)} = [{session.ShortSessionSpan?.ToString(SessionSpanFormat) ?? "?"} / {session.FullSessionSpan?.ToString(SessionSpanFormat) ?? "?"}] {session.SessionFirstStart.EventAsString} -> {session.SessionLastEnd.EventAsString}");
            
            prevSession = session;
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
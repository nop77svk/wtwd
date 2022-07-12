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
        IEnumerable<PcStateChange> result = new List<PcStateChange>();

        string sinceAsStr = since.ToUniversalTime().ToString("O");
        XNamespace eventLogNS = "http://schemas.microsoft.com/win/2004/08/events/event";

        EventLogQuery? queryKernelBoot = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and (EventID = 20 or EventID = 25) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelBoot != null)
        {
            IEnumerable<PcStateChange> queryEvents = queryKernelBoot
                .AsEnumerable()
                .Where(x => x.TimeCreated != null)
                .Select(x => new PcStateChange(
                    PcStateChangeHow.ShutdownOrStartup,
                    x.Id is 20 or 25 ? PcStateChangeWhat.On : PcStateChangeWhat.Unknown,
                    x.TimeCreated ?? DateTime.Now
                ));
            result = result.Concat(queryEvents);
        }

        EventLogQuery? queryKernelGeneral = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-General' and (EventID = 12 or EventID = 13) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelGeneral != null)
        {
            IEnumerable<PcStateChange> queryEvents = queryKernelGeneral
                .AsEnumerable()
                .Where(x => x.TimeCreated != null)
                .Select(x => new PcStateChange(
                    PcStateChangeHow.ShutdownOrStartup,
                    x.Id switch
                    {
                        12 => PcStateChangeWhat.On,
                        13 => PcStateChangeWhat.Off,
                        _ => PcStateChangeWhat.Unknown
                    },
                    x.TimeCreated ?? DateTime.Now
                ));
            result = result.Concat(queryEvents);
        }

        EventLogQuery? queryKernelPower = new EventLogQuery("System", PathType.LogName, $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Power' and (EventID = 109 or EventID = 42 or EventID = 107) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelPower != null)
        {
            IEnumerable<PcStateChange> queryEvents = queryKernelPower
                .AsEnumerable()
                .Where(x => x.TimeCreated != null)
                .Select(x => new PcStateChange(
                    x.Id switch
                    {
                        109 => PcStateChangeHow.ShutdownOrStartup,
                        42 or 107 => PcStateChangeHow.SleepOrWakeUp,
                        _ => PcStateChangeHow.Unknown
                    },
                    x.Id switch
                    {
                        42 or 109 => PcStateChangeWhat.Off,
                        107 => PcStateChangeWhat.On,
                        _ => PcStateChangeWhat.Unknown
                    },
                    x.TimeCreated ?? DateTime.Now
                ));
            result = result.Concat(queryEvents);
        }

        EventLogQuery? querySynTpEnhServiceForLockUnlock = new EventLogQuery("Application", PathType.LogName, @"Event[System[Provider/@Name = 'SynTPEnhService' and EventID = 0] and EventData/Data]");
        if (querySynTpEnhServiceForLockUnlock != null)
        {
            IEnumerable<PcStateChange> queryEvents = querySynTpEnhServiceForLockUnlock
                .AsEnumerable()
                .Where(x => x.TimeCreated != null)
                .Where(x => x.TimeCreated >= since)
                .Select(x => new ValueTuple<EventRecord, string>(
                    x,
                    XDocument.Parse(x.ToXml())
                        .Descendants(eventLogNS + "Event")
                        .Descendants(eventLogNS + "EventData")
                        .Descendants(eventLogNS + "Data")
                        .Select(x => x.Value)
                        .Where(x => x.StartsWith("Session Changed User ", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault(string.Empty)
                ))
                .Where(x => !string.IsNullOrEmpty(x.Item2))
                .Select(x => new PcStateChange(
                    PcStateChangeHow.LockOrUnlock,
                    x.Item2.EndsWith(" lock", StringComparison.OrdinalIgnoreCase)
                        ? PcStateChangeWhat.Off
                        : x.Item2.EndsWith(" unlock", StringComparison.OrdinalIgnoreCase)
                            ? PcStateChangeWhat.On
                            : PcStateChangeWhat.Unknown,
                    x.Item1.TimeCreated ?? DateTime.Now
                ));
            result = result.Concat(queryEvents);
        }

        return result;
    }
}
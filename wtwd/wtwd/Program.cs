#pragma warning disable CA1416
namespace wtwd;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

internal class Program
{
    static void Main(string[] args)
    {
        IEnumerable<PcStateChange> allEvents = ReadEventLogForPcStateChanges(DateTime.Now.AddMonths(-1));
        IEnumerable<PcSession> allSessions = CalculateSessions(allEvents);

        foreach (var row in allEvents.OrderBy(x => x.When))
        {
            Console.WriteLine($"[{row.When}] {row.EventAsString}");
        }

        Console.WriteLine("Hello, World!");
    }

    private static IEnumerable<PcStateChange> ReadEventLogForPcStateChanges(DateTime since)
    {
        string sinceAsStr = since.ToUniversalTime().ToString("O");
        IEnumerable<PcStateChange> result = new List<PcStateChange>();

        EventLogQuery? queryKernelBoot = new EventLogQuery("System", PathType.LogName, $"*[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and (EventID = 20 or EventID = 25) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryKernelBoot != null)
        {
            IEnumerable<PcStateChange> queryEvents = queryKernelBoot
                .AsEnumerable()
                .Where(x => x.TimeCreated != null)
                .Select(x => new PcStateChange(PcStateChangeHow.ShutdownOrStartup, x.Id is 20 or 25 ? PcStateChangeWhat.On : PcStateChangeWhat.Unknown, x.TimeCreated ?? DateTime.Now, x));
            result = result.Concat(queryEvents);
        }

        EventLogQuery? queryKernelGeneral = new EventLogQuery("System", PathType.LogName, $"*[System[Provider/@Name = 'Microsoft-Windows-Kernel-General' and (EventID = 12 or EventID = 13) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
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
                    x.TimeCreated ?? DateTime.Now,
                    x
                ));
            result = result.Concat(queryEvents);
        }

        EventLogQuery? queryKernelPower = new EventLogQuery("System", PathType.LogName, $"*[System[Provider/@Name = 'Microsoft-Windows-Kernel-Power' and (EventID = 109 or EventID = 42 or EventID = 107) and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
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
                    x.TimeCreated ?? DateTime.Now,
                    x
                ));
            result = result.Concat(queryEvents);
        }

        return result;
    }

    private static IEnumerable<PcSession> CalculateSessions(IEnumerable<PcStateChange> pcStateChanges)
    {
        throw new NotImplementedException("Feature not yet implemented");
    }
}
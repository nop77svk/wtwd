#pragma warning disable CA1416
namespace NoP77svk.wtwd.Model.Xform;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Linq;
using NoP77svk.wtwd.Model;

public static class WindowsEventToStateChange
{
    public static PcStateChange AsPcStateChange(this EventLogRecord evnt)
    {
        return (evnt.LogName, evnt.ProviderName) switch
        {
            ("System", "Microsoft-Windows-Kernel-Boot") => FromKernelBootEvent(evnt),
            ("System", "Microsoft-Windows-Kernel-General") => FromKernelGeneralEvent(evnt),
            ("System", "Microsoft-Windows-Kernel-Power") => FromKernelPowerEvent(evnt),
            ("Application", "SynTPEnhService") => FromSynTPEnhServiceEvent(evnt),
            (LockUnlockEventLog.LogName, LockUnlockEventLog.SourceName) => FromWTWD(evnt),
            _ => new PcStateChange(new PcStateChangeEvent(PcStateChangeHow.Unknown, PcStateChangeWhat.Unknown), DateTime.Now)
        };
    }

    public static PcStateChange AsPcStateChange(this EventRecord evnt)
    {
        if (evnt is EventLogRecord)
            return ((EventLogRecord)evnt).AsPcStateChange();
        else
            throw new ArgumentOutOfRangeException(nameof(evnt), $"Argument not of {nameof(EventLogRecord)} class");
    }

    private static PcStateChange FromKernelBootEvent(EventLogRecord evnt)
    {
        if (evnt.Id is 20 or 25 or 27)
            return new PcStateChange(new PcStateChangeEvent(PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.On), evnt.TimeCreated ?? DateTime.Now);
        else
            return new PcStateChange(new PcStateChangeEvent(PcStateChangeHow.Unknown, PcStateChangeWhat.Unknown), evnt.TimeCreated ?? DateTime.Now);
    }

    private static PcStateChange FromKernelGeneralEvent(EventLogRecord evnt)
    {
        return new PcStateChange(
            new PcStateChangeEvent(
                evnt.Id switch
                {
                    12 or 13 => PcStateChangeHow.ShutdownOrStartup,
                    _ => PcStateChangeHow.Unknown,
                },
                evnt.Id switch
                {
                    12 => PcStateChangeWhat.On,
                    13 => PcStateChangeWhat.Off,
                    _ => PcStateChangeWhat.Unknown
                }
            ),
            evnt.TimeCreated ?? DateTime.Now
        );
    }

    private static PcStateChange FromKernelPowerEvent(EventLogRecord evnt)
    {
        return new PcStateChange(
            new PcStateChangeEvent(
                evnt.Id switch
                {
                    109 or 27 => PcStateChangeHow.ShutdownOrStartup,
                    42 => PcStateChangeHow.Hibernate,
                    107 or 506 or 507 => PcStateChangeHow.SleepOrWakeUp,
                    _ => PcStateChangeHow.Unknown
                },
                evnt.Id switch
                {
                    42 or 109 or 506 => PcStateChangeWhat.Off,
                    107 or 507 or 27 => PcStateChangeWhat.On,
                    _ => PcStateChangeWhat.Unknown
                }
            ),
            evnt.TimeCreated ?? DateTime.Now
        );
    }

    private static PcStateChange FromSynTPEnhServiceEvent(EventLogRecord evnt)
    {
        PcStateChange result;
        string relevantUserDataPrefix = @"Session Changed User ";

        string eventAsXml = evnt.ToXml();
        string? relevantEventData = XDocument.Parse(eventAsXml)
            .Descendants(EventLogConst.XmlNS + "Event")
            .Descendants(EventLogConst.XmlNS + "EventData")
            .Descendants(EventLogConst.XmlNS + "Data")
            .Select(x => x.Value)
            .Where(x => x.StartsWith(relevantUserDataPrefix, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(string.Empty);

        if (string.IsNullOrEmpty(relevantEventData) || evnt.TimeCreated == null)
        {
            result = new PcStateChange(new PcStateChangeEvent(PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.Unknown), evnt.TimeCreated ?? DateTime.Now);
        }
        else
        {
            relevantEventData = relevantEventData[relevantUserDataPrefix.Length..];
            result = new PcStateChange(
                new PcStateChangeEvent(
                    PcStateChangeHow.LockOrUnlock,
                    relevantEventData.ToLower() switch
                    {
                        "lock" => PcStateChangeWhat.Off,
                        "unlock" => PcStateChangeWhat.On,
                        "log off" => PcStateChangeWhat.Off,
                        "log on" => PcStateChangeWhat.On,
                        _ => PcStateChangeWhat.Unknown
                    }
                ),
                evnt.TimeCreated ?? DateTime.Now
            );
        }

        return result;
    }

    private static PcStateChange FromWTWD(EventLogRecord evnt)
    {
        return new PcStateChange(
            new PcStateChangeEvent(
                PcStateChangeHow.LockOrUnlock,
                evnt.Id switch
                {
                    LockUnlockEventLog.LockEventId => PcStateChangeWhat.Off,
                    LockUnlockEventLog.UnlockEventId => PcStateChangeWhat.On,
                    _ => PcStateChangeWhat.Unknown
                }
            ),
            evnt.TimeCreated ?? DateTime.Now
        );
    }
}

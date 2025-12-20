namespace NoP77svk.wtwd.cli.List;
#pragma warning disable CA1416

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Xml.Linq;

using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Model.Xform;
using NoP77svk.wtwd.Utilities;

internal static class EventLogProcessor
{
    internal static IEnumerable<EventRecord> GetEventLogsSince(DateTime since)
    {
        string sinceAsStr = since.ToUniversalTime().ToString("O");

        IEnumerable<EventRecord> result = Enumerable.Empty<EventRecord>();

        string queryKernelBootStr = $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and {EventIdsOrExpanded(20, 25, 27)} and TimeCreated/@SystemTime >= '{sinceAsStr}']]";
        EventLogQuery queryKernelBoot = new EventLogQuery("System", PathType.LogName, queryKernelBootStr);
        result = result.Concat(queryKernelBoot.AsEnumerable());

        string queryKernelGeneralStr = $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-General' and {EventIdsOrExpanded(12, 13)} and TimeCreated/@SystemTime >= '{sinceAsStr}']]";
        EventLogQuery queryKernelGeneral = new EventLogQuery("System", PathType.LogName, queryKernelGeneralStr);
        result = result.Concat(queryKernelGeneral.AsEnumerable());

        string queryKernelPowerStr = $"Event[System[Provider/@Name = 'Microsoft-Windows-Kernel-Power' and {EventIdsOrExpanded(109, 42, 107, 506, 507)} and TimeCreated/@SystemTime >= '{sinceAsStr}']]";
        EventLogQuery queryKernelPower = new EventLogQuery("System", PathType.LogName, queryKernelPowerStr);
        result = result.Concat(queryKernelPower.AsEnumerable());

        string querySynTpEnhServiceForLockUnlockStr = $"Event[System[Provider/@Name = 'SynTPEnhService' and {EventIdsOrExpanded(0)}] and EventData/Data]";
        EventLogQuery querySynTpEnhServiceForLockUnlock = new EventLogQuery("Application", PathType.LogName, querySynTpEnhServiceForLockUnlockStr);
        result = result
            .Concat(querySynTpEnhServiceForLockUnlock.AsEnumerable()
                .Where(evnt => evnt.TimeCreated >= since)
            );

        string queryExplicitWtwdLockUnlockStr = @$"Event[System[Provider/@Name = '{LockUnlockEventLog.SourceName}' and Task = {LockUnlockEventLog.LockUnlockCategory} and TimeCreated/@SystemTime >= '{sinceAsStr}']]";
        EventLogQuery queryExplicitWtwdLockUnlock = new EventLogQuery(LockUnlockEventLog.LogName, PathType.LogName, queryExplicitWtwdLockUnlockStr);

        WindowsUser osUser = WindowsUser.Current();
        var queryExplicitWtwdLockUnlockTimeFiltered = queryExplicitWtwdLockUnlock.AsEnumerable()
            .Where(evnt => evnt.TimeCreated >= since)
            .Select(evnt => new ValueTuple<EventRecord, ValueTuple<string?, string?, string?>>(
                evnt,
                XDocument.Parse(evnt.ToXml())
                    .Descendants(EventLogConst.XmlNS + "Event")
                    .Descendants(EventLogConst.XmlNS + "EventData")
                    .Descendants(EventLogConst.XmlNS + "Data")
                    .Where(node => node.Value.StartsWith(LockUnlockEventLog.EventDataUserDomainPrefix)
                        || node.Value.StartsWith(LockUnlockEventLog.EventDataUserNamePrefix)
                        || node.Value.StartsWith(LockUnlockEventLog.EventDataUserSIDPrefix)
                    )
                    .Select(node => new ValueTuple<string?, string?, string?>(
                        node.Value.StartsWith(LockUnlockEventLog.EventDataUserDomainPrefix) ? node.Value.Substring(LockUnlockEventLog.EventDataUserDomainPrefix.Length).Trim() : null,
                        node.Value.StartsWith(LockUnlockEventLog.EventDataUserNamePrefix) ? node.Value.Substring(LockUnlockEventLog.EventDataUserNamePrefix.Length).Trim() : null,
                        node.Value.StartsWith(LockUnlockEventLog.EventDataUserSIDPrefix) ? node.Value.Substring(LockUnlockEventLog.EventDataUserSIDPrefix.Length).Trim() : null
                    ))
                    .Aggregate(
                        seed: new ValueTuple<string?, string?, string?>(null, null, null),
                        func: (accumulator, value) => (accumulator.Item1 ?? value.Item1, accumulator.Item2 ?? value.Item2, accumulator.Item3 ?? value.Item3)
                    )
            ))
            .Where(evntPlus => evntPlus.Item2.Item1 == osUser.Domain && evntPlus.Item2.Item2 == osUser.Name
                || evntPlus.Item2.Item3 == osUser.SID
            )
            .Select(evntPlus => evntPlus.Item1);

        result = result.Concat(queryExplicitWtwdLockUnlockTimeFiltered);

        return result.Where(evnt => evnt.TimeCreated >= since);
    }

    internal static IEnumerable<PcSession> ToPcSessions(this IEnumerable<EventRecord> events, TimeSpan? trimSessionsUnder, bool allowMachineOnlySessions)
    {
        return events
            .Select(evnt => evnt.AsPcStateChange())
            .Where(stch => stch.Event.How != PcStateChangeHow.Unknown && stch.Event.What != PcStateChangeWhat.Unknown)
            .StateChangesToSessions()
            .Where(session => session.IsStillRunning || session.FullSessionSpan != TimeSpan.Zero)
            .Where(session => trimSessionsUnder == null
                || session.ShortSessionSpan >= trimSessionsUnder
                || session.IsStillRunning
            )
            .Where(session => allowMachineOnlySessions
                || session.SessionLastStart.Event.How == PcStateChangeHow.LockOrUnlock
                || session.SessionFirstEnd?.Event.How == PcStateChangeHow.LockOrUnlock
                || session.IsStillRunning
            );
    }

    private static string EventIdsOrExpanded(params int[] ids)
    {
        IEnumerable<string> idPredicates = ids.Select(id => $"EventID = {id}");

        string result = string.Join(" or ", idPredicates);

        if (ids.Length > 1)
        {
            result = $"({result})";
        }

        return result;
    }
}

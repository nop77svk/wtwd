#pragma warning disable CA1416
namespace NoP77svk.wtwd.cli.List;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;
using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;
using NoP77svk.wtwd.Model.Xform;

public static class ListProgram
{
    private const string SessionDisplayIndent = "    ";
    private const string DayFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";
    private const string SessionSpanMinutesFormat = @"hh\:mm";
    private const string SessionSpanHoursFormat = @"hh\:mm";
    private const string SessionSpanDaysFormat = @"d\d\ hh\:mm";

    public static void Execute(ListCli cli)
    {
        Execute(ListConfig.FromRawCLI(cli));
    }

    internal static void Execute(ListConfig cli)
    {
        DateTime logsSince = DateTime.Now.AddMonths(-1);
        TimeSpan roundingInterval = TimeSpan.FromMinutes(1);

        IEnumerable<PcSession> pcSessions = GetEventLogsSince(logsSince)
            .Select(evnt => evnt.AsPcStateChange())
            .Where(stch => stch.Event.How != PcStateChangeHow.Unknown && stch.Event.What != PcStateChangeWhat.Unknown)
            .StateChangesToSessions()
            .Where(session => session.IsStillRunning || session.FullSessionSpan != TimeSpan.Zero)
            .Where(session => cli.TrimSessionsUnder == null
                || session.ShortSessionSpan >= cli.TrimSessionsUnder
                || session.IsStillRunning
            )
            .Where(session => cli.AllowMachineOnlySessions
                || session.SessionLastStart.Event.How == PcStateChangeHow.LockOrUnlock
                || session.SessionFirstEnd?.Event.How == PcStateChangeHow.LockOrUnlock
                || session.IsStillRunning
            );

        if (cli.TrimBreaksUnder != null)
        {
            pcSessions = pcSessions
                .OrderBy(session => session.SessionFirstStart.When)
                .RecognizeElementRuns((current, lagged) => current == null && lagged == null
                    || current != null && lagged != null
                        && lagged.SessionFirstEnd != null
                        && current.SessionLastStart.When.Subtract(lagged.SessionFirstEnd.When) < cli.TrimBreaksUnder
                )
                .GroupBy(
                    keySelector: x => x.RunId,
                    elementSelector: x => x.Element
                )
                .Select(grp => grp.OrderBy(session => session.SessionFirstStart.When))
                .Select(orderedSessions => orderedSessions.First().MergeWith(orderedSessions.Last()));
        }

        DisplayTheSessions(pcSessions, roundingInterval);
    }

    private static void DisplayTheSessions(IEnumerable<PcSession> sessions, TimeSpan roundingInterval)
    {
        var sessionsGroupedByDay = sessions
            .GroupBy(session => session.SessionLastStart.When.Date)
            .OrderBy(sessionGroup => sessionGroup.Key);

        foreach (var sessionDayGroup in sessionsGroupedByDay)
        {
            Console.WriteLine(sessionDayGroup.Key.ToString(DayFormat));

            var daySessionsOrdered = sessionDayGroup
                .OrderBy(session => session.SessionLastStart.When);

            foreach (var session in daySessionsOrdered)
            {
                string msg = session.ToDisplayString(roundingInterval);
                Console.WriteLine($"{SessionDisplayIndent}{msg}");
            }
        }
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

    private static string FormatDayOffset(DateTime start, DateTime end)
    {
        int offset = (int)Math.Round(end.Date.Subtract(start.Date).TotalDays);

        string result = offset switch
        {
            < 0 => $"/{offset:d}",
            > 0 => $"/+{offset:d}",
            0 => string.Empty
        };

        return result;
    }

    private static IEnumerable<EventRecord> GetEventLogsSince(DateTime since)
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

    private static string ToDisplayString(this PcSession session, TimeSpan roundingInterval)
    {
        DateTime sessionAnchorTstamp = session.SessionLastStart.When;

        StringBuilder result = new StringBuilder();

        result.Append("(");
        result.Append(session.SessionFirstStart.When.Round(roundingInterval).ToString(TimeFormat));
        result.Append(FormatDayOffset(sessionAnchorTstamp, session.SessionFirstStart.When));
        result.Append(") ");

        result.Append(session.SessionLastStart.When.Round(roundingInterval).ToString(TimeFormat));

        result.Append(" -> ");

        if (session.SessionLastEnd == null || session.SessionFirstEnd == null)
        {
            result.Append($"(ongoing session from {string.Join('+', session.StartEventsOrdered.Select(x => x.AsString))})");
        }
        else
        {
            result.Append(session.SessionFirstEnd.When.Round(roundingInterval).ToString(TimeFormat));
            result.Append(FormatDayOffset(sessionAnchorTstamp, session.SessionFirstEnd.When));

            result.Append(" (");
            result.Append(session.SessionLastEnd.When.Round(roundingInterval).ToString(TimeFormat));
            result.Append(FormatDayOffset(sessionAnchorTstamp, session.SessionLastEnd.When));
            result.Append(")");
        }

        if (session.SessionLastEnd != null && session.SessionFirstEnd != null)
        {
            result.Append(" = ");

            string? shortSessionSpanDisp = session.FullSessionSpan?.Add(TimeSpan.FromMinutes(1))
                .ToVariableString(minutesFormat: SessionSpanMinutesFormat, hoursFormat: SessionSpanHoursFormat, daysFormat: SessionSpanDaysFormat);

            string? longSessionSpanDisp = session.FullSessionSpan?.Add(TimeSpan.FromMinutes(1))
                .ToVariableString(minutesFormat: SessionSpanMinutesFormat, hoursFormat: SessionSpanHoursFormat, daysFormat: SessionSpanDaysFormat);

            result.Append(shortSessionSpanDisp ?? "?");
            if (shortSessionSpanDisp != longSessionSpanDisp)
            {
                result.Append(" [");
                result.Append(longSessionSpanDisp ?? "?");
                result.Append("]");
            }

            result.Append(" (");
            result.Append(string.Join('+', session.StartEventsOrdered.Select(x => x.AsString)));
            result.Append(" -> ");
            result.Append(string.Join('+', session.EndEventsOrdered.Select(x => x.AsString)));
            result.Append(")");
        }

        return result.ToString();
    }
}

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
    private const string NextDayFormat = "(yyyy-MM-dd) HH:mm";
    private const string TimeFormat = "HH:mm";
    private const string SessionSpanFormat = @"hh\:mm";

    public static void Execute(ListCLI cli)
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
        PcSession? prevSession = null;
        foreach (var session in sessions)
        {
            if (prevSession == null || session.SessionFirstStart.When.Date != prevSession.SessionFirstStart.When.Date)
                System.Console.WriteLine($"{session.SessionFirstStart.When.ToString(DayFormat)}");

            StringBuilder msg = new StringBuilder(SessionDisplayIndent);
            msg.Append("(");
            msg.Append(session.SessionFirstStart.When.Round(roundingInterval).ToString(TimeFormat));
            msg.Append(") ");
            msg.Append(session.SessionLastStart.When.Round(roundingInterval).ToString(TimeFormat));
            msg.Append(" -> ");

            if (session.SessionLastEnd == null || session.SessionFirstEnd == null)
            {
                msg.Append($"(ongoing session from {string.Join('+', session.StartEventsOrdered.Select(x => x.AsString))})");
            }
            else if (session.SessionLastEnd.When.Date != session.SessionFirstStart.When.Date)
            {
                msg.Append(session.SessionFirstEnd.When.Round(roundingInterval).ToString(TimeFormat));
                msg.Append("[");
                msg.Append(session.SessionFirstEnd.When.Date.Subtract(session.SessionLastStart.When.Date).ToString(@"\+d"));
                msg.Append("]");

                msg.Append(" (");
                msg.Append(session.SessionLastEnd.When.Round(roundingInterval).ToString(TimeFormat));
                msg.Append(")");
            }
            else
            {
                msg.Append(session.SessionFirstEnd.When.Round(roundingInterval).ToString(TimeFormat));
                msg.Append(" (");
                msg.Append(session.SessionLastEnd.When.Round(roundingInterval).ToString(TimeFormat));
                msg.Append(")");
            }

            if (session.SessionLastEnd != null && session.SessionFirstEnd != null)
            {
                msg.Append(" = ");

                string? shortSessionSpanDisp = session.FullSessionSpan?.Add(TimeSpan.FromMinutes(1)).ToVariableString(minutesFormat: @"hh\:mm", hoursFormat: @"hh\:mm", daysFormat: @"d\d\ hh\:mm");
                string? longSessionSpanDisp = session.FullSessionSpan?.Add(TimeSpan.FromMinutes(1)).ToVariableString(minutesFormat: @"hh\:mm", hoursFormat: @"hh\:mm", daysFormat: @"d\d\ hh\:mm");

                msg.Append(shortSessionSpanDisp ?? "?");
                if (shortSessionSpanDisp != longSessionSpanDisp)
                {
                    msg.Append(" [");
                    msg.Append(longSessionSpanDisp ?? "?");
                    msg.Append("]");
                }

                msg.Append(" (");
                msg.Append(string.Join('+', session.StartEventsOrdered.Select(x => x.AsString)));
                msg.Append(" -> ");
                msg.Append(string.Join('+', session.EndEventsOrdered.Select(x => x.AsString)));
                msg.Append(")");
            }

            System.Console.WriteLine(msg.ToString());
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
        {
            unionedEvents = unionedEvents.Concat(querySynTpEnhServiceForLockUnlock.AsEnumerable()
                .Where(evnt => evnt.TimeCreated >= since)
            );
        }

        EventLogQuery? queryExplicitWtwdLockUnlock = new EventLogQuery(LockUnlockEventLog.LogName, PathType.LogName, @$"Event[System[Provider/@Name = '{LockUnlockEventLog.SourceName}' and Task = {LockUnlockEventLog.LockUnlockCategory} and TimeCreated/@SystemTime >= '{sinceAsStr}']]");
        if (queryExplicitWtwdLockUnlock != null)
        {
            WindowsUser user = WindowsUser.Current();

            unionedEvents = unionedEvents.Concat(queryExplicitWtwdLockUnlock.AsEnumerable()
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
                .Where(evntPlus => evntPlus.Item2.Item1 == user.Domain && evntPlus.Item2.Item2 == user.Name
                    || evntPlus.Item2.Item3 == user.SID
                )
                .Select(evntPlus => evntPlus.Item1)
            );
        }

        return unionedEvents
            .Where(evnt => evnt.TimeCreated >= since);
    }
}

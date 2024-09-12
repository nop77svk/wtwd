namespace NoP77svk.wtwd.cli.List;

using System.Text;

using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;

internal static class PcSessionDisplayExt
{
    private const string TimeFormat = "HH:mm";
    private const string SessionSpanMinutesFormat = @"hh\:mm";
    private const string SessionSpanHoursFormat = @"hh\:mm";
    private const string SessionSpanDaysFormat = @"d\d\ hh\:mm";

    internal static string ToHumanReadableString(this PcSession session, TimeSpan roundingInterval)
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
            result.Append(string.Join('+', session.EndEventsOrdered?.Select(x => x.AsString) ?? Enumerable.Empty<string>()));
            result.Append(")");
        }

        return result.ToString();
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
}
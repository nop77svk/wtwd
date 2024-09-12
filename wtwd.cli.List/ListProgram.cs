#pragma warning disable CA1416
namespace NoP77svk.wtwd.cli.List;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;

public class ListProgram
{
    private const string SessionDisplayIndent = "    ";
    private const string DayFormat = "yyyy-MM-dd";

    public ListConfig Config { get; init; }

    public ListProgram(ListConfig config)
    {
        Config = config;
    }

    public ListProgram(ListCli cli)
        : this(ListConfig.FromRawCLI(cli))
    {
    }

    public void Execute()
    {
        DateTime logsSince = DateTime.Now.AddMonths(-1);
        TimeSpan roundingInterval = TimeSpan.FromMinutes(1);

        IEnumerable<PcSession> pcSessions = EventLogProcessor.GetEventLogsSince(logsSince)
            .ToPcSessions(Config.TrimSessionsUnder, Config.AllowMachineOnlySessions);

        if (Config.TrimBreaksUnder != null)
        {
            pcSessions = pcSessions
                .OrderBy(session => session.SessionFirstStart.When)
                .RecognizeElementRuns((current, lagged) => current == null && lagged == null
                    || current != null && lagged != null
                        && lagged.SessionFirstEnd != null
                        && current.SessionLastStart.When.Subtract(lagged.SessionFirstEnd.When) < Config.TrimBreaksUnder
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

    private static void DisplayTheSessionsHumanReadablePrettyPrint(IEnumerable<PcSession> sessions, TimeSpan roundingInterval)
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
                string msg = session.ToHumanReadableString(roundingInterval);
                Console.WriteLine($"{SessionDisplayIndent}{msg}");
            }
        }
    }

    private static void DisplayTheSessionsJSON(IEnumerable<PcSession> sessions)
    {
        IEnumerable<JsonDisplayPcSessionDto> sessionAsJsonDto = sessions
            .Select(session => new JsonDisplayPcSessionDto(session.SessionLastStart.When, session.SessionFirstEnd?.When ?? DateTime.Now)
            {
                FirstStart = session.SessionFirstStart.When,
                StartEventsOrdered = session.StartEventsOrdered.Select(evnt => evnt.AsString),
                LastEnd = session.SessionLastEnd?.When,
                EndEventsOrdered = session.EndEventsOrdered?.Select(evnt => evnt.AsString)
            })
            .OrderBy(dto => dto.Start);

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string sessionAsJsonString = JsonSerializer.Serialize(sessionAsJsonDto, serializerOptions);

        Console.WriteLine(sessionAsJsonString);
    }

    private void DisplayTheSessions(IEnumerable<PcSession> sessions, TimeSpan roundingInterval)
    {
        if (Config.OutputFormat == ListOutputFormat.PrettyPrint)
        {
            DisplayTheSessionsHumanReadablePrettyPrint(sessions, roundingInterval);
        }
        else if (Config.OutputFormat == ListOutputFormat.JSON)
        {
            DisplayTheSessionsJSON(sessions);
        }
        else
        {
            throw new NotImplementedException($"Don't know how to {Config.OutputFormat.ToString()}-print the read PC sessions");
        }
    }
}

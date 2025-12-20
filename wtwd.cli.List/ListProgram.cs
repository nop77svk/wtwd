namespace NoP77svk.wtwd.cli.List;
#pragma warning disable CA1416

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using CsvHelper;
using CsvHelper.Configuration;

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

    private static void DisplayTheSessionsTimeRecWorkunitsCSV(IEnumerable<PcSession> sessions)
    {
        List<PcSession> sessionsMaterialized = sessions.ToList();

        var idleWorkunits = sessionsMaterialized
            .OrderBy(session => session.SessionLastStart.When)
            .Lag()
            .Where(x => x.Lagged?.SessionFirstEnd?.When.Date == x.Current.SessionLastStart.When.Date // lagged session must be from the same day as the current session
                && x.Lagged?.SessionFirstEnd?.When.Date == x.Lagged?.SessionLastStart.When.Date // lagged session must not span multiple days
            )
            .Select(x => new TimeRecCsvDisplayPcSessionDto(x.Lagged!.SessionLastEnd!.When, x.Current.SessionFirstStart.When)
            {
                TaskId = "<idle>"
            });

        IEnumerable<TimeRecCsvDisplayPcSessionDto> workunits = sessionsMaterialized
            .Select(session => new TimeRecCsvDisplayPcSessionDto(session.SessionLastStart.When, session.SessionFirstEnd?.When ?? DateTime.Now))
            .Concat(idleWorkunits)
            .OrderBy(workunit => workunit.CheckIn);

        CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            AllowComments = false,
            Delimiter = ",",
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
            IncludePrivateMembers = true,
            Mode = CsvMode.RFC4180,
            NewLine = "\n",
            Quote = '"'
        };

        using CsvWriter csvWriter = new CsvWriter(Console.Out, csvConfiguration);

        csvWriter.WriteRecords(workunits);
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

        JsonWriterOptions jsonWriterOptions = new JsonWriterOptions()
        {
            Encoder = JavaScriptEncoder.Default,
            Indented = true,
            SkipValidation = false
        };

        using Stream consoleOutStream = Console.OpenStandardOutput();

        using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(consoleOutStream, jsonWriterOptions);

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        JsonSerializer.Serialize(jsonWriter, sessionAsJsonDto, serializerOptions);
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
        else if (Config.OutputFormat == ListOutputFormat.TimeRecWorkunitsCSV)
        {
            DisplayTheSessionsTimeRecWorkunitsCSV(sessions);
        }
        else
        {
            throw new NotImplementedException($"Don't know how to {Config.OutputFormat.ToString()}-print the read PC sessions");
        }
    }
}

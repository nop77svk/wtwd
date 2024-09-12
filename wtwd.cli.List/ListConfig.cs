namespace NoP77svk.wtwd.cli.List;
using NoP77svk.wtwd.Utilities;

public class ListConfig
{
    internal static ListConfig FromRawCLI(ListCli cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanExt.Parse(cli.TrimSessionsUnder),
            TrimBreaksUnder = TimeSpanExt.Parse(cli.TrimBreaksUnder),
            AllowMachineOnlySessions = !cli.IgnoreMachineOnlySessions,
            OutputFormat = RecognizeOutputFormat(cli.Format)
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    internal TimeSpan? TrimBreaksUnder { get; init; }

    internal bool AllowMachineOnlySessions { get; init; }

    internal ListOutputFormat OutputFormat { get; init; }

    private ListConfig()
    {
    }

    private static ListOutputFormat RecognizeOutputFormat(string? outputFormatCli)
    {
        ListOutputFormat result = ListOutputFormat.PrettyPrint;

        if (Enum.TryParse(outputFormatCli?.Replace("-", string.Empty), true, out ListOutputFormat outputFormatParsed))
        {
            result = outputFormatParsed;
        }

        return result;
    }
}

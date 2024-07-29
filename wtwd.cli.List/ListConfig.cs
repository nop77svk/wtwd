namespace NoP77svk.wtwd.cli.List;
using NoP77svk.wtwd.Utilities;

internal class ListConfig
{
    internal static ListConfig FromRawCLI(ListCli cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanExt.Parse(cli.TrimSessionsUnder),
            TrimBreaksUnder = TimeSpanExt.Parse(cli.TrimBreaksUnder),
            AllowMachineOnlySessions = !cli.IgnoreMachineOnlySessions
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    internal TimeSpan? TrimBreaksUnder { get; init; }

    internal bool AllowMachineOnlySessions { get; init; }

    private ListConfig()
    {
    }
}

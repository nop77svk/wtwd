namespace wtwd.cli.List;
using wtwd.Utilities;

internal class ListConfig
{
    internal static ListConfig FromRawCLI(ListCLI cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanExt.Parse(cli.TrimSessionsUnder),
            AllowMachineOnlySessions = !cli.IgnoreMachineOnlySessions
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    internal bool AllowMachineOnlySessions { get; init; }

    private ListConfig()
    {
    }
}

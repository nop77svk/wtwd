namespace wtwd.cli.List;
using wtwd.Utilities;

internal class ListConfig
{
    internal static ListConfig FromRawCLI(ListCLI cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanExt.Parse(cli.TrimSessionsUnder),
            IgnoreSessionsWoUnlock = cli.IgnoreSessionsWoUnlock
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    internal bool IgnoreSessionsWoUnlock { get; init; }

    private ListConfig()
    {
    }
}

namespace wtwd.cli.List;
using wtwd.Utilities;

internal class ListConfig
{
    internal static ListConfig FromRawCLI(ListCLI cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanFromString.Parse(cli.TrimSessionsUnder)
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    private ListConfig()
    {
    }
}

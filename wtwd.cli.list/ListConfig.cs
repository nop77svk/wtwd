namespace wtwd.cli.list;
using wtwd.utilities;

public class ListConfig
{
    public static ListConfig FromRawCLI(ListCLI cli)
    {
        return new ListConfig()
        {
            TrimSessionsUnder = TimeSpanFromString.Parse(cli.TrimSessionsUnder)
        };
    }

    public TimeSpan? TrimSessionsUnder { get; init; }

    private ListConfig()
    {
    }
}

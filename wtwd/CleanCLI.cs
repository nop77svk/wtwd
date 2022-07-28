namespace wtwd;
using wtwd.utilities;

internal class CleanCLI
{
    internal static CleanCLI FromRawCLI(RawCLI cli)
    {
        return new CleanCLI()
        {
            TrimSessionsUnder = TimeSpanFromString.Parse(cli.TrimSessionsUnder)
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }
}

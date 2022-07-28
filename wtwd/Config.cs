namespace wtwd;
using wtwd.Utilities;

internal class Config
{
    internal static Config FromRawCLI(CLI cli)
    {
        return new Config()
        {
            TrimSessionsUnder = TimeSpanFromString.Parse(cli.TrimSessionsUnder)
        };
    }

    internal TimeSpan? TrimSessionsUnder { get; init; }

    private Config()
    {
    }
}

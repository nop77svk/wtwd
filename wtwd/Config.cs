namespace wtwd;
using wtwd.utilities;

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
}

namespace wtwd;
using CommandLine;
using wtwd.utilities;

internal class CLI
{
    [Option("trim-sessions-under", Required = false, Default = "3:30", HelpText = ""
        + "\nSessions shorter than the supplied threshold are automatically discarded."
        + "\nUse decimal values with the suffix of \"s\" (seconds), \"m\" (minutes), \"h\" (hours) or a mm:ss time span specification.")]
    public string? TrimSessionsUnderStr { get; set; }

    private bool _isParsedTrimSessionUnder = false;
    private TimeSpan? _trimSessionUnder = null;
    public TimeSpan? TrimSessionsUnder
    {
        get
        {
            if (!_isParsedTrimSessionUnder)
            {
                _trimSessionUnder = TimeSpanFromString.Parse(TrimSessionsUnderStr);
                _isParsedTrimSessionUnder = true;
            }

            return _trimSessionUnder;
        }
    }
}

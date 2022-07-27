namespace wtwd;
using CommandLine;

internal class CLI
{
    [Option("trim-sessions-under", Required = false, HelpText = ""
        + "\nSessions shorter than the supplied threshold are automatically discarded."
        + "\nUse integer values with the suffix of \"s\" (seconds), \"m\" (minutes), \"h\" (hours) or a mm:ss time span specification.")]
    public string? TrimSessionsUnder { get; set; }
}

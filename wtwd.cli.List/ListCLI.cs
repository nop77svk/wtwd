namespace NoP77svk.wtwd.cli.List;

using CommandLine;

[Verb("list", isDefault: true, HelpText = "\nMine the Windows event log for the PC sessions and list them")]
public class ListCli
{
    [Option("trim-sessions-under", Required = false, Default = "3:30", HelpText = ""
        + "\nSessions shorter than the supplied threshold are automatically discarded."
        + "\nUse decimal values with the suffix of \"s\" (seconds), \"m\" (minutes), \"h\" (hours) or a mm:ss time span specification.")]
    public string? TrimSessionsUnder { get; set; }

    [Option("trim-breaks-under", Required = false, Default = "1:30", HelpText = ""
        + "\nBreaks shorter than the supplied threshold are automatically discarded and the surrounding sessions are merged."
        + "\nUse decimal values with the suffix of \"s\" (seconds), \"m\" (minutes), \"h\" (hours) or a mm:ss time span specification.")]
    public string? TrimBreaksUnder { get; set; }

    [Option("ignore-machine-only-sessions", Required = false, HelpText = "Sessions without starting unlock event may be considered purely technical/maintenance. When supplied, supposed \"machine-only\" sessions are not displayed.")]
    public bool IgnoreMachineOnlySessions { get; set; } = false;

    [Option("format", Required = false, HelpText = "Output format")]
    public string? Format { get; set; }
}

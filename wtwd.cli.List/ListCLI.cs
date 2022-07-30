namespace wtwd.cli.List;
using CommandLine;

[Verb("list", isDefault: true, HelpText = "\nMine the Windows event log for the PC sessions and list them")]
public class ListCLI
{
    [Option("trim-sessions-under", Required = false, Default = "3:30", HelpText = ""
        + "\nSessions shorter than the supplied threshold are automatically discarded."
        + "\nUse decimal values with the suffix of \"s\" (seconds), \"m\" (minutes), \"h\" (hours) or a mm:ss time span specification.")]
    public string? TrimSessionsUnder { get; set; }

    [Option("ignore-no-unlock-sessions", Required = false, Default = false, HelpText = "\nSessions without starting unlock event may be considered purely technical. When switched ON, such sessions are not displayed.")]
    public bool IgnoreSessionsWoUnlock { get; set; } = false;
}

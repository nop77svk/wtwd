namespace NoP77svk.wtwd.cli.InitLockUnlock;
using CommandLine;

[Verb("init-lock-unlock", HelpText = "Initialize Windows event log for explicit/custom lock/unlock events source")]
public class InitLockUnlockCLI
{
    [Option('x', "exe-path", Required = false, HelpText = "Path to wtwd.exe.\nIf not supplied, path to this executable being run is used.")]
    public string? ExeFilePath { get; set; }
}

namespace wtwd;
using CommandLine;
using wtwd.cli.List;
using wtwd.cli.Lock;
using wtwd.cli.Unlock;
using wtwd.cli.InitLockUnlock;

internal class Program
{
    internal static int Main(string[] args)
    {
        Parser.Default
            .ParseArguments<ListCLI, LockCLI, UnlockCLI, InitLockUnlockCLI>(args)
            .WithParsed<ListCLI>(cli => ListProgram.Execute(cli))
            .WithParsed<LockCLI>(cli => LockProgram.Execute(cli))
            .WithParsed<UnlockCLI>(cli => UnlockProgram.Execute(cli))
            .WithParsed<InitLockUnlockCLI>(cli => InitLockUnlockProgram.Execute(cli));

        return 0;
    }
}

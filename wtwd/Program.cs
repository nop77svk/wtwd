namespace NoP77svk.wtwd;
using CommandLine;
using NoP77svk.wtwd.cli.List;
using NoP77svk.wtwd.cli.Lock;
using NoP77svk.wtwd.cli.Unlock;
using NoP77svk.wtwd.cli.InitLockUnlock;

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

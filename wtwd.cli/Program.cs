namespace NoP77svk.wtwd.cli;

using CommandLine;

using NoP77svk.wtwd.cli.List;
using NoP77svk.wtwd.cli.Lock;
using NoP77svk.wtwd.cli.Unlock;
using NoP77svk.wtwd.cli.InitLockUnlock;
using NoP77svk.wtwd.Utilities;

internal static class Program
{
    internal static int Main(string[] args)
    {
        WindowsVersionChecks();

        Parser.Default
            .ParseArguments<ListCli, LockCli, UnlockCli, InitLockUnlockCLI>(args)
            .WithParsed<ListCli>(cli => ListProgram.Execute(cli))
            .WithParsed<LockCli>(cli => LockProgram.Execute(cli))
            .WithParsed<UnlockCli>(cli => UnlockProgram.Execute(cli))
            .WithParsed<InitLockUnlockCLI>(cli => InitLockUnlockProgram.Execute(cli));

        return 0;
    }

    internal static void WindowsVersionChecks()
    {
        if (!WindowsVersion.IsTestedVersion)
        {
            if (WindowsVersion.IsCompatibleVersion)
                Console.Error.WriteLine($"WARNING:\n{Environment.OSVersion.VersionString} is assumed to be compatible with WTWD, but has not yet been tested!\n");
            else
                throw new NotImplementedException($"{Environment.OSVersion.VersionString} is assumed to be incompatible with WTWD!");
        }
    }
}

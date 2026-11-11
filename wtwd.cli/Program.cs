namespace NoP77svk.TRecs.cli;

using CommandLine;

using NoP77svk.TRecs.cli.InitLockUnlock;
using NoP77svk.TRecs.cli.List;
using NoP77svk.TRecs.cli.Lock;
using NoP77svk.TRecs.cli.Unlock;
using NoP77svk.TRecs.Utilities;
using NoP77svk.TRecs.Utilities.Windows;

internal static class Program
{
    internal static int Main(string[] args)
    {
        WindowsVersionChecks();

        Parser.Default
            .ParseArguments<ListCli, LockCli, UnlockCli, InitLockUnlockCli>(args)
            .WithParsed<ListCli>(cli => new ListProgram(cli).Execute())
            .WithParsed<LockCli>(cli => LockProgram.Execute(cli))
            .WithParsed<UnlockCli>(cli => UnlockProgram.Execute(cli))
            .WithParsed<InitLockUnlockCli>(cli => InitLockUnlockProgram.Execute(cli));

        return 0;
    }

    internal static void WindowsVersionChecks()
    {
        if (!WindowsVersion.IsTestedVersion)
        {
            if (WindowsVersion.IsCompatibleVersion)
            {
                Console.Error.WriteLine($"WARNING:\n{Environment.OSVersion.VersionString} is assumed to be compatible with WTWD, but has not yet been tested!\n");
            }
            else
            {
                throw new PlatformNotSupportedException($"{Environment.OSVersion.VersionString} is assumed to be incompatible with WTWD!");
            }
        }
    }
}

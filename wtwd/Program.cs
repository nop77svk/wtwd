namespace wtwd;
using CommandLine;
using wtwd.cli.List;
using wtwd.cli.Lock;
using wtwd.cli.Unlock;

internal class Program
{
    internal static int Main(string[] args)
    {
        Parser.Default
            .ParseArguments<ListCLI, LockCLI>(args)
            .WithParsed<ListCLI>(cli => ListProgram.Execute(cli))
            .WithParsed<LockCLI>(cli => LockProgram.Execute(cli))
            .WithParsed<UnlockCLI>(cli => UnlockProgram.Execute(cli));

        return 0;
    }
}

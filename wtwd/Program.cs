namespace wtwd;
using CommandLine;
using wtwd.cli.list;
using wtwd.cli.lock;

internal class Program
{
    internal static int Main(string[] args)
    {
        Parser.Default
            .ParseArguments<ListCLI, LockCLI>(args)
            .WithParsed<ListCLI>(cli => ListProgram.Execute(ListConfig.FromRawCLI(cli)))
            .WithParsed<LockCLI>(cli => LockProgram.Execute(LockConfig.FromRawCLI(cli)));

        return 0;
    }
}
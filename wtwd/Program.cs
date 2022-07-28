namespace wtwd;
using CommandLine;
using wtwd.cli.List;
using wtwd.cli.Lock;

internal class Program
{
    internal static int Main(string[] args)
    {
        Parser.Default
            .ParseArguments<ListCLI, LockCLI>(args)
            .WithParsed<ListCLI>(cli => ListProgram.Execute(cli))
            .WithParsed<LockCLI>(cli => LockProgram.Execute(cli));

        return 0;
    }
}
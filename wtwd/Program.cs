namespace wtwd;
using CommandLine;
using wtwd.cli.list;

internal class Program
{
    internal static int Main(string[] args)
    {
        Parser.Default
            .ParseArguments<ListCLI>(args)
            .WithParsed(cli => ListProgram.Execute(ListConfig.FromRawCLI(cli)));

        return 0;
    }
}
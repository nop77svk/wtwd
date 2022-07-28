namespace wtwd.cli.Lock;

public static class LockProgram
{
    public static void Execute(LockCLI cli)
    {
        Execute(LockConfig.FromRawCLI(cli));
    }

    internal static void Execute(LockConfig cli)
    {
    }
}

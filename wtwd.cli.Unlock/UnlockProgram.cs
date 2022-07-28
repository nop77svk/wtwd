namespace wtwd.cli.Unlock;

public static class UnlockProgram
{
    public static void Execute(UnlockCLI cli)
    {
        Execute(UnlockConfig.FromRawCLI(cli));
    }

    internal static void Execute(UnlockConfig cli)
    {
    }
}

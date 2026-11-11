namespace NoP77svk.TRecs.cli.InitLockUnlock;

using NoP77svk.TRecs.Utilities.Windows;

internal class InitLockUnlockConfig
{
    internal string ExeFilePath { get; init; } = string.Empty;

    private InitLockUnlockConfig()
    {
    }

    internal static InitLockUnlockConfig FromRawCLI(InitLockUnlockCli cli)
    {
        return new InitLockUnlockConfig()
        {
            ExeFilePath = cli.ExeFilePath ?? WtwdProcess.ExeFileName
        };
    }
}

namespace NoP77svk.wtwd.cli.InitLockUnlock;
using NoP77svk.wtwd.Utilities;

internal class InitLockUnlockConfig
{
    internal string ExeFilePath { get; init; } = string.Empty;

    private InitLockUnlockConfig()
    {
    }

    internal static InitLockUnlockConfig FromRawCLI(InitLockUnlockCLI cli)
    {
        return new InitLockUnlockConfig()
        {
            ExeFilePath = cli.ExeFilePath ?? WtwdProcess.ExeFileName
        };
    }
}

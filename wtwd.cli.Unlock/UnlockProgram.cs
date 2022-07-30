#pragma warning disable CA1416
namespace wtwd.cli.Unlock;

using System.Diagnostics;
using System.Security;
using wtwd.Model;

public static class UnlockProgram
{
    public static void Execute(UnlockCLI cli)
    {
        Execute(UnlockConfig.FromRawCLI(cli));
    }

    internal static void Execute(UnlockConfig cli)
    {
        try
        {
            EventLog.WriteEntry(LockUnlockEventLog.SourceName, LockUnlockEventLog.UnlockMessage, EventLogEntryType.Information, LockUnlockEventLog.UnlockEventId, LockUnlockEventLog.LockUnlockCategory);
            Console.WriteLine("Unlock event successfully logged");
        }
        catch (SecurityException e)
        {
            throw new SecurityException(LockUnlockEventLog.NeedElevatedPrivilegesErrorMsg, e);
        }
    }
}

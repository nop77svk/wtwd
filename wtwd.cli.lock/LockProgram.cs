#pragma warning disable CA1416
namespace wtwd.cli.Lock;
using System.Diagnostics;
using System.Security;
using wtwd.Model;

public static class LockProgram
{
    public static void Execute(LockCLI cli)
    {
        Execute(LockConfig.FromRawCLI(cli));
    }

    internal static void Execute(LockConfig cli)
    {
        try
        {
            EventLog.WriteEntry(LockUnlockEventLog.SourceName, LockUnlockEventLog.LockMessage, EventLogEntryType.Information, LockUnlockEventLog.LockEventId, LockUnlockEventLog.LockUnlockCategory);
            Console.WriteLine("Lock event successfully logged");
        }
        catch (SecurityException e)
        {
            throw new SecurityException(LockUnlockEventLog.NeedElevatedPrivilegesErrorMsg, e);
        }
    }
}

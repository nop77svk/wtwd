#pragma warning disable CA1416
namespace wtwd.cli.Lock;
using System.Diagnostics;
using System.Security;
using wtwd.Model;
using wtwd.Utilities;
using wtwd.Model.Xform;

public static class LockProgram
{
    public static void Execute(LockCLI cli)
    {
        Execute(LockConfig.FromRawCLI(cli));
    }

    internal static void Execute(LockConfig cli)
    {
        WindowsUser user = WindowsUser.Current();
        object[] eventData = user.AsEventData().ToArray();

        try
        {
            // note: It's not nice to log user name+domain into EventData, but .NET 6's EventLog API does not log UserID, so...
            EventInstance eventInstance = new EventInstance(LockUnlockEventLog.LockEventId, LockUnlockEventLog.LockUnlockCategory, EventLogEntryType.Information);
            EventLog.WriteEvent(LockUnlockEventLog.SourceName, eventInstance, LockUnlockEventLog.LockMessage, eventData.Length > 0 ? eventData[0] : null, eventData.Length > 1 ? eventData[1] : null, eventData.Length > 2 ? eventData[2] : null);

            Console.WriteLine("Lock event successfully logged");
        }
        catch (SecurityException e)
        {
            throw new SecurityException(LockUnlockEventLog.NeedElevatedPrivilegesErrorMsg, e);
        }
    }
}

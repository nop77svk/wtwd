#pragma warning disable CA1416
namespace NoP77svk.wtwd.cli.Unlock;

using System.Diagnostics;
using System.Security;
using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;
using NoP77svk.wtwd.Model.Xform;

public static class UnlockProgram
{
    public static void Execute(UnlockCLI cli)
    {
        Execute(UnlockConfig.FromRawCLI(cli));
    }

    internal static void Execute(UnlockConfig cli)
    {
        WindowsUser user = WindowsUser.Current();
        object[] eventData = user.AsEventData().ToArray();

        try
        {
            // note: It's not nice to log user name+domain into EventData, but .NET 6's EventLog API does not log UserID, so...
            EventInstance eventInstance = new EventInstance(LockUnlockEventLog.UnlockEventId, LockUnlockEventLog.LockUnlockCategory, EventLogEntryType.Information);
            EventLog.WriteEvent(LockUnlockEventLog.SourceName, eventInstance, LockUnlockEventLog.UnlockMessage, eventData.Length > 0 ? eventData[0] : null, eventData.Length > 1 ? eventData[1] : null, eventData.Length > 2 ? eventData[2] : null);

            Console.WriteLine("Unlock event successfully logged");
        }
        catch (SecurityException e)
        {
            throw new SecurityException(LockUnlockEventLog.NeedElevatedPrivilegesErrorMsg, e);
        }
    }
}

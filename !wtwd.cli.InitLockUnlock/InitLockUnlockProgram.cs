#pragma warning disable CA1416
namespace wtwd.cli.InitLockUnlock;

using System.Diagnostics;
using System.Security;
using wtwd.Model;

public static class InitLockUnlockProgram
{
    public static void Execute(InitLockUnlockCLI cli)
    {
        Execute(InitLockUnlockConfig.FromRawCLI(cli));
    }

    internal static void Execute(InitLockUnlockConfig cli)
    {
        EventSourceCreationData eventSource = new EventSourceCreationData(LockUnlockEventLog.SourceName, LockUnlockEventLog.LogName)
        {
            MachineName = LockUnlockEventLog.LocalMachine
        };

        if (!EventLog.SourceExists(LockUnlockEventLog.SourceName))
        {
            try
            {
                EventLog.CreateEventSource(eventSource);
                Console.WriteLine("Event log source successfully initialized");
            }
            catch (SecurityException e)
            {
                throw new SecurityException("Please, re-run again with elevated privileges", e);
            }
        }
        else
        {
            Console.WriteLine("Event log source already initialized");
        }
    }
}

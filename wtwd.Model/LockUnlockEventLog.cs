namespace NoP77svk.wtwd.Model;

public static class LockUnlockEventLog
{
    public const string LocalMachine = ".";
    public const string LogName = "Application";
    public const string SourceName = "WTWD";

    public const string NeedElevatedPrivilegesErrorMsg = "Cannot write custom event logs.\n\nPlease, re-run (with elevated privileges) as\n\n\twtwd init-lock-unlock\n\nto set up new event log source for explicit lock/unlock logging";

    public const int LockUnlockCategory = 1;

    public const int LockEventId = 0;
    public const string LockMessage = "Explicit workstation lock";

    public const int UnlockEventId = 1;
    public const string UnlockMessage = "Explicit workstation unlock";

    public const string EventDataUserNamePrefix = "user.name:";
    public const string EventDataUserDomainPrefix = "user.domain:";
    public const string EventDataUserSIDPrefix = "user.sid:";
}

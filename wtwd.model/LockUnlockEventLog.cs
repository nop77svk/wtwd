namespace wtwd.Model;

public static class LockUnlockEventLog
{
    public const string LocalMachine = ".";
    public const string LogName = "Application";
    public const string SourceName = "WTWD";

    public const string NeedElevatedPrivilegesErrorMsg = "Cannot write custom event logs.\n\nPlease, re-run (with elevated privileges) as\n\n\twtwd init-lock-unlock\n\nto set up new event log source for explicit lock/unlock logging";
}

namespace NoP77svk.TRecs.Model.Xform;

using NoP77svk.TRecs.Model;
using NoP77svk.TRecs.Utilities.Windows;

public static class WindowsUserToEventData
{
    public static IEnumerable<string> AsEventData(this WindowsUser user)
    {
        if (!string.IsNullOrEmpty(user.Domain) || !string.IsNullOrEmpty(user.Name))
        {
            yield return $"{LockUnlockEventLog.EventDataUserDomainPrefix}{user.Domain}";
            yield return $"{LockUnlockEventLog.EventDataUserNamePrefix}{user.Name}";
        }

        if (!string.IsNullOrEmpty(user.SID))
        {
            yield return $"{LockUnlockEventLog.EventDataUserSIDPrefix}{user.SID}";
        }
    }
}

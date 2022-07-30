namespace NoP77svk.wtwd.Model.Xform;

using NoP77svk.wtwd.Model;
using NoP77svk.wtwd.Utilities;

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

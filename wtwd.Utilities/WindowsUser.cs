#pragma warning disable CA1416
#pragma warning disable SA1313
namespace NoP77svk.wtwd.Utilities;
using System.Security.Principal;

public record WindowsUser(string Domain, string Name, string? SID)
{
    public string DomainUser => $"{Domain}\\{Name}";

    public static WindowsUser Current()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        string[] nameSplit = identity.Name.Split('\\', 2);

        return new WindowsUser(
            nameSplit.Length > 0 ? nameSplit[0] : Environment.UserDomainName,
            nameSplit.Length > 1 ? nameSplit[1] : Environment.UserName,
            (identity.User?.IsAccountSid() ?? false) ? identity.User?.AccountDomainSid?.ToString() : null
        );
    }
}

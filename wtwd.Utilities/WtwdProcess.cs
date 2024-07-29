namespace NoP77svk.wtwd.Utilities;
using System.Diagnostics;

public static class WtwdProcess
{
    public static string ExeFileName { get => CurrentProcessFileName ?? CurrentDomainFriendlyName; }
    internal static string? CurrentProcessFileName { get => Process.GetCurrentProcess().MainModule?.FileName; }
    internal static string CurrentDomainFriendlyName { get => AppDomain.CurrentDomain.FriendlyName; }
}

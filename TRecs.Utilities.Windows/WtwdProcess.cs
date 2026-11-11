namespace NoP77svk.TRecs.Utilities.Windows;

using System.Diagnostics;

public static class WtwdProcess
{
    public static string ExeFileName => CurrentProcessFileName ?? CurrentDomainFriendlyName;
    internal static string? CurrentProcessFileName => Process.GetCurrentProcess().MainModule?.FileName;
    internal static string CurrentDomainFriendlyName => AppDomain.CurrentDomain.FriendlyName;
}

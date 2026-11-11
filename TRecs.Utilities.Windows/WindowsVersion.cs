namespace NoP77svk.TRecs.Utilities.Windows;

using System;

public static class WindowsVersion
{
    public static bool IsTestedVersion => OperatingSystem.IsWindowsVersionAtLeast(10);

    public static bool IsCompatibleVersion => OperatingSystem.IsWindowsVersionAtLeast(6, 1);
}

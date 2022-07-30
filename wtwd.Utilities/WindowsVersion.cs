namespace NoP77svk.wtwd.Utilities;
using System;

public static class WindowsVersion
{
    public static bool IsTestedVersion { get => OperatingSystem.IsWindowsVersionAtLeast(10); }

    public static bool IsCompatibleVersion { get => OperatingSystem.IsWindowsVersionAtLeast(6, 1); }
}

namespace wtwd;
using System.Diagnostics.Eventing.Reader;

internal class Program
{
    static void Main(string[] args)
    {
        string oneMonthAgo = DateTime.Now.AddMonths(-1).ToUniversalTime().ToString("O");
        EventLogQuery? queryKernelBoot = new EventLogQuery("System", PathType.LogName, $"*[System[Provider/@Name = 'Microsoft-Windows-Kernel-Boot' and (EventID = 20 or EventID = 25) and TimeCreated/@SystemTime >= '{oneMonthAgo}']]");

        foreach (var row in queryKernelBoot.AsEnumerable())
        {
            Console.WriteLine(row);
        }

        Console.WriteLine("Hello, World!");
    }
}
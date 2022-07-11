#pragma warning disable SA1313
namespace wtwd;
using System.Diagnostics.Eventing.Reader;

internal record PcStateChange(PcStateChangeHow How, PcStateChangeWhat What, DateTime When, EventRecord OriginalEvent)
{
    internal string? EventAsString
    {
        get => (How, What) switch
        {
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.On) => "startup",
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.Off) => "shutdown",
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.On) => "wakeup",
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.Off) => "sleep",
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.On) => "unlock",
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.Off) => "lock",
            _ => null
        };
    }
}

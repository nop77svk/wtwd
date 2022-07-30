#pragma warning disable SA1313, CA1416
namespace wtwd.Model;

public record PcStateChangeEvent(PcStateChangeHow How, PcStateChangeWhat What)
{
    public string? AsString
    {
        get => (How, What) switch
        {
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.On) => "startup",
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.Off) => "shutdown",
            (PcStateChangeHow.Hibernate, PcStateChangeWhat.On) => "kickstart",
            (PcStateChangeHow.Hibernate, PcStateChangeWhat.Off) => "hibernate",
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.On) => "wakeup",
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.Off) => "sleep",
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.On) => "unlock",
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.Off) => "lock",
            _ => null
        };
    }
}

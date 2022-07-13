#pragma warning disable SA1313, CA1416
namespace wtwd.model;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Linq;

public record PcStateChange(PcStateChangeHow How, PcStateChangeWhat What, DateTime When)
{
    public string? EventAsString
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

namespace NoP77svk.wtwd.Model;
#pragma warning disable SA1313

public record PcStateChangeEvent(PcStateChangeHow How, PcStateChangeWhat What)
{
    public PcStateChangeEventName AsEventName
        => (How, What) switch
        {
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.On) => PcStateChangeEventName.Startup,
            (PcStateChangeHow.ShutdownOrStartup, PcStateChangeWhat.Off) => PcStateChangeEventName.Shutdown,
            (PcStateChangeHow.Hibernate, PcStateChangeWhat.On) => PcStateChangeEventName.KickStart,
            (PcStateChangeHow.Hibernate, PcStateChangeWhat.Off) => PcStateChangeEventName.Hibernate,
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.On) => PcStateChangeEventName.WakeUp,
            (PcStateChangeHow.SleepOrWakeUp, PcStateChangeWhat.Off) => PcStateChangeEventName.Sleep,
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.On) => PcStateChangeEventName.Unknown,
            (PcStateChangeHow.LockOrUnlock, PcStateChangeWhat.Off) => PcStateChangeEventName.Lock,
            _ => PcStateChangeEventName.Unknown
        };

    public string AsString => this.AsEventName.ToString();
}

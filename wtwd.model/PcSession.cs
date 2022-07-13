namespace wtwd.model;

public class PcSession
{
    public PcStateChange SessionFirstStart { get; private set; }
    public PcStateChange SessionLastStart { get; private set; }
    public PcStateChange? SessionFirstEnd { get; private set; }
    public PcStateChange? SessionLastEnd { get; private set; }

    public TimeSpan IdleStartSpan { get => SessionLastStart.When.Subtract(SessionFirstStart.When); }
    public bool IsStillRunning { get => SessionLastEnd == null && SessionFirstEnd == null; }
    public TimeSpan? ShortSessionSpan { get => (SessionFirstEnd ?? SessionLastEnd)?.When.Subtract(SessionLastStart.When); }
    public TimeSpan? FullSessionSpan { get => (SessionLastEnd ?? SessionFirstEnd)?.When.Subtract(SessionFirstStart.When); }
    public TimeSpan IdleEndSpan { get => SessionLastEnd?.When.Subtract(SessionFirstEnd?.When ?? DateTime.Now) ?? TimeSpan.Zero; }

    public PcSession(PcStateChange sessionStart)
    {
        if (sessionStart.What != PcStateChangeWhat.On)
            throw new ArgumentOutOfRangeException(nameof(sessionStart) + "." + nameof(sessionStart.What), sessionStart.What.ToString());

        SessionFirstStart = sessionStart;
        SessionLastStart = sessionStart;
    }

    public void ResolveEvent(PcStateChange evnt)
    {
        if (evnt.What == PcStateChangeWhat.On)
            ResolveStartEvent(evnt);
        else if (evnt.What == PcStateChangeWhat.Off)
            ResolveEndEvent(evnt);
    }

    private void ResolveStartEvent(PcStateChange evnt)
    {
        if (evnt.What != PcStateChangeWhat.On)
            throw new ArgumentOutOfRangeException(nameof(evnt) + "." + nameof(evnt.What), evnt.What.ToString());

        if (SessionFirstStart == null && SessionLastStart == null)
        {
            SessionFirstStart = evnt;
            SessionLastStart = evnt;
        }
        else if (SessionFirstStart == null || SessionLastStart == null)
        {
            throw new EInvalidSessionMarkerEvent("Invalid state of session start markers");
        }
        else
        {
            if (evnt.When < SessionFirstStart.When || evnt.When == SessionFirstStart.When && evnt.How < evnt.How)
                SessionFirstStart = evnt;
            
            if (evnt.When > SessionLastStart.When || evnt.When == SessionLastStart.When && evnt.How > evnt.How)
                SessionLastStart = evnt;
        }
    }

    private void ResolveEndEvent(PcStateChange evnt)
    {
        if (evnt.What != PcStateChangeWhat.Off)
            throw new ArgumentOutOfRangeException(nameof(evnt) + "." + nameof(evnt.What), evnt.What.ToString());

        if (SessionFirstEnd == null && SessionLastEnd == null)
        {
            SessionFirstEnd = evnt;
            SessionLastEnd = evnt;
        }
        else if (SessionFirstEnd == null || SessionLastEnd == null)
        {
            throw new EInvalidSessionMarkerEvent("Invalid state of session end markers");
        }
        else
        {
            if (evnt.When < SessionFirstEnd.When || evnt.When == SessionFirstEnd.When && evnt.How < evnt.How)
                SessionFirstEnd = evnt;
            
            if (evnt.When > SessionLastEnd.When || evnt.When == SessionLastEnd.When && evnt.How > evnt.How)
                SessionLastEnd = evnt;
        }
    }
}

namespace wtwd;

internal class PcSession
{
    internal PcStateChange SessionFirstStart { get; private set; }
    internal PcStateChange SessionLastStart { get; private set; }
    internal PcStateChange? SessionFirstEnd { get; private set; }
    internal PcStateChange? SessionLastEnd { get; private set; }

    internal TimeSpan IdleStartSpan { get => SessionLastStart.When.Subtract(SessionFirstStart.When); }
    internal bool IsStillRunning { get => SessionLastEnd == null && SessionFirstEnd == null; }
    internal TimeSpan? ShortSessionSpan { get => (SessionFirstEnd ?? SessionLastEnd)?.When.Subtract(SessionLastStart.When); }
    internal TimeSpan? FullSessionSpan { get => (SessionLastEnd ?? SessionFirstEnd)?.When.Subtract(SessionFirstStart.When); }
    internal TimeSpan IdleEndSpan { get => SessionLastEnd?.When.Subtract(SessionFirstEnd?.When ?? DateTime.Now) ?? TimeSpan.Zero; }

    internal PcSession(PcStateChange sessionStart)
    {
        if (sessionStart.What != PcStateChangeWhat.On)
            throw new ArgumentOutOfRangeException(nameof(sessionStart) + "." + nameof(sessionStart.What), sessionStart.What.ToString());

        SessionFirstStart = sessionStart;
        SessionLastStart = sessionStart;
    }

    internal void ResolveEvent(PcStateChange evnt)
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

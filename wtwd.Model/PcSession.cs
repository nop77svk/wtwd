namespace NoP77svk.wtwd.Model;

public class PcSession
{
    private SortedList<DateTime, PcStateChange> _sessionStart = new ();
    private SortedList<DateTime, PcStateChange> _sessionEnd = new ();

    public PcStateChange SessionFirstStart => _sessionStart.First().Value;
    public PcStateChange SessionLastStart => _sessionStart.Last().Value;
    public PcStateChange? SessionFirstEnd => _sessionEnd.Any() ? _sessionEnd.First().Value : null;
    public PcStateChange? SessionLastEnd => _sessionEnd.Any() ? _sessionEnd.Last().Value : null;

    public TimeSpan IdleStartSpan => SessionLastStart.When.Subtract(SessionFirstStart.When);
    public IEnumerable<PcStateChangeEvent> StartEventsOrdered
        => _sessionStart
            .DistinctBy(x => x.Value.Event)
            .OrderBy(x => x.Value.When)
            .Select(x => x.Value.Event);

    public bool IsStillRunning => !_sessionEnd.Any();
    public TimeSpan? ShortSessionSpan => SessionFirstEnd?.When.Subtract(SessionLastStart.When);
    public TimeSpan? FullSessionSpan => SessionLastEnd?.When.Subtract(SessionFirstStart.When);
    public TimeSpan IdleEndSpan => SessionLastEnd?.When.Subtract(SessionFirstEnd?.When ?? DateTime.Now) ?? TimeSpan.Zero;
    public IEnumerable<PcStateChangeEvent>? EndEventsOrdered
        => _sessionEnd.Any()
            ? _sessionEnd
                .DistinctBy(x => x.Value.Event)
                .OrderBy(x => x.Value.When)
                .Select(x => x.Value.Event)
            : null;

    public PcSession()
    {
    }

    public PcSession(PcStateChange sessionStart)
    {
        ResolveEvent(sessionStart);
    }

    public PcSession MergeWith(PcSession otherSession)
    {
        PcSession result = new PcSession()
        {
            _sessionStart = this._sessionStart,
            _sessionEnd = otherSession._sessionEnd
        };

        return result;
    }

    public void ResolveEvent(PcStateChange evnt)
    {
        if (evnt.Event.What == PcStateChangeWhat.On)
        {
            ResolveStartEvent(evnt);
        }
        else if (evnt.Event.What == PcStateChangeWhat.Off)
        {
            ResolveEndEvent(evnt);
        }
    }

    private void ResolveStartEvent(PcStateChange evnt)
    {
        if (evnt.Event.What != PcStateChangeWhat.On)
        {
            throw new ArgumentOutOfRangeException(nameof(evnt) + "." + nameof(evnt.Event), evnt.Event.ToString());
        }

        if (_sessionEnd.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(evnt) + "." + nameof(evnt.Event), $"Cannot add start-event {evnt.Event} to the session when there already are end-events");
        }

        if (_sessionStart.ContainsKey(evnt.When))
        {
            if (_sessionStart[evnt.When].Event.How < evnt.Event.How)
            {
                _sessionStart[evnt.When] = evnt;
            }
        }
        else
        {
            _sessionStart.Add(evnt.When, evnt);
        }
    }

    private void ResolveEndEvent(PcStateChange evnt)
    {
        if (evnt.Event.What != PcStateChangeWhat.Off)
        {
            throw new ArgumentOutOfRangeException(nameof(evnt) + "." + nameof(evnt.Event), evnt.Event.ToString());
        }

        if (_sessionEnd.ContainsKey(evnt.When))
        {
            if (_sessionEnd[evnt.When].Event.How > evnt.Event.How)
            {
                _sessionEnd[evnt.When] = evnt;
            }
        }
        else
        {
            _sessionEnd.Add(evnt.When, evnt);
        }
    }
}

namespace NoP77svk.wtwd.Model.Xform;
using NoP77svk.wtwd.Model;

public static class PcStateChangeExt
{
    public static IEnumerable<PcSession> StateChangesToSessions(this IEnumerable<PcStateChange> pcStateChanges)
    {
        PcStateChangeWhat previousState = PcStateChangeWhat.Unknown;
        PcSession? result = null;

        foreach (PcStateChange evnt in pcStateChanges
            .Where(x => x.Event.What is PcStateChangeWhat.On or PcStateChangeWhat.Off)
            .OrderBy(x => x.When)
            .SkipWhile(stch => stch.Event.What == PcStateChangeWhat.Off)
        )
        {
            if (result == null)
            {
                result = new PcSession(evnt);
            }
            else if (evnt.Event.What == previousState || evnt.Event.What == PcStateChangeWhat.Off)
            {
                result.ResolveEvent(evnt);
            }
            else if (evnt.Event.What == PcStateChangeWhat.On)
            {
                yield return result;
                result = new PcSession(evnt);
            }
            else
            {
                throw new InvalidSessionMarkerEventException($"Something wrong has happened in {nameof(StateChangesToSessions)}");
            }

            previousState = evnt.Event.What;
        }

        if (result != null)
        {
            yield return result;
        }
    }
}

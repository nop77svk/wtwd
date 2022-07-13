namespace wtwd;
using wtwd.model;

internal static class PcStateChangeExt
{
    internal static IEnumerable<PcSession> StateChangesToSessions(this IEnumerable<PcStateChange> pcStateChanges)
    {
        PcStateChangeWhat previousState = PcStateChangeWhat.Unknown;
        PcSession? result = null;

        foreach (PcStateChange evnt in pcStateChanges
            .Where(x => x.What is PcStateChangeWhat.On or PcStateChangeWhat.Off)
            .OrderBy(x => x.When)
        )
        {
            if (result == null)
            {
                result = new PcSession(evnt);
            }
            else if (evnt.What == previousState || evnt.What == PcStateChangeWhat.Off)
            {
                result.ResolveEvent(evnt);
            }
            else if (evnt.What == PcStateChangeWhat.On)
            {
                yield return result;
                result = new PcSession(evnt);
            }
            else
            {
                throw new EInvalidSessionMarkerEvent($"Something wrong has happened in {nameof(StateChangesToSessions)}");
            }

            previousState = evnt.What;
        }

        if (result != null)
            yield return result;
    }


}

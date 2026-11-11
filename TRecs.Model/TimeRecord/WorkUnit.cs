namespace NoP77svk.TRecs.Model.TimeRecord;

using System;

public class WorkUnit
{
    public WorkUnit(WorkUnitStartEvent startInclusive, WorkUnitEndEvent? endExclusive)
    {
        if (endExclusive is not null && startInclusive >= endExclusive)
        {
            throw new ArgumentException($"Start event time {startInclusive} must be less than end event time {endExclusive}.", nameof(startInclusive));
        }

        StartInclusive = startInclusive;
        EndExclusive = endExclusive;
    }

    public WorkUnitStartEvent StartInclusive { get; }
    public WorkUnitEndEvent? EndExclusive { get; }
    public TimeSpan Duration => CalculateDuration(EndExclusive?.EventTime);
    public bool IsOpen => EndExclusive is null;

    public TimeSpan CalculateDuration(DateTimeOffset? endExclusiveTime)
        => endExclusiveTime is null
            ? DateTimeOffset.Now - StartInclusive.EventTime
            : endExclusiveTime.Value - StartInclusive.EventTime;
}

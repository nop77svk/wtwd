namespace TRecs.Model.TimeRecord;

using System;
using System.Collections.Generic;
using System.Linq;

using NoP77svk.Linq;

public class WorkUnitCollection
    : List<WorkUnit>
{
    // 2do! constructor with IEnumerable<WorkUnit> parameter
    public double TotalDurationInMilliseconds
        => this.OrderBy(workUnit => workUnit.StartInclusive)
        .Lead()
        .Select(lead => lead.Current.CalculateDuration(lead.Current.EndExclusive?.EventTime ?? lead.Leading?.StartInclusive.EventTime))
        .Sum(eventDuration => eventDuration.TotalMilliseconds);

    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(TotalDurationInMilliseconds);
}

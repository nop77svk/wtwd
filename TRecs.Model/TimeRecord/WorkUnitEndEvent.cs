#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name
#pragma warning disable S4035 // Implement IEqualityComparer<T> or seal the (abstract) class
namespace NoP77svk.TRecs.Model.TimeRecord;

using System;

public class WorkUnitEndEvent : WorkUnitEvent
{
    public WorkUnitEndEvent(DateTimeOffset eventTime)
        : base(eventTime)
    {
    }
}

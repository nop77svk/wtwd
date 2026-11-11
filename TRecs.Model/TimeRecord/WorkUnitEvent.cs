#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name
#pragma warning disable S4035 // Implement IEqualityComparer<T> or seal the (abstract) class
namespace NoP77svk.TRecs.Model.TimeRecord;

using System;
using System.Globalization;

public abstract class WorkUnitEvent
    : IComparable<WorkUnitEvent>, IEquatable<WorkUnitEvent>
{
    protected WorkUnitEvent(DateTimeOffset eventTime)
    {
        EventTime = eventTime;
    }

    public DateTimeOffset EventTime { get; set; }

    public int CompareTo(WorkUnitEvent other) => EventTime.CompareTo(other.EventTime);
    public bool Equals(WorkUnitEvent other) => EventTime.Equals(other.EventTime);
    public override bool Equals(object obj) => obj is WorkUnitEvent other && Equals(other);
    public override int GetHashCode() => EventTime.GetHashCode();
    public override string ToString() => EventTime.ToString(CultureInfo.CurrentCulture);

    public static bool operator <(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime < right.EventTime;
    public static bool operator <=(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime <= right.EventTime;
    public static bool operator >(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime > right.EventTime;
    public static bool operator >=(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime >= right.EventTime;
    public static bool operator ==(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime == right.EventTime;
    public static bool operator !=(WorkUnitEvent left, WorkUnitEvent right) => left.EventTime != right.EventTime;
}

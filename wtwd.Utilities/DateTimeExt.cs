// source: https://stackoverflow.com/questions/1393696/rounding-datetime-objects
namespace wtwd.Utilities;

public static class DateTimeExt
{
    public static DateTime Ceil(this DateTime dateTime, TimeSpan interval)
    {
        long overflow = dateTime.Ticks % interval.Ticks;
        return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
    }

    public static DateTime Floor(this DateTime dateTime, TimeSpan interval)
    {
        long overflow = dateTime.Ticks % interval.Ticks;
        return dateTime.AddTicks(-overflow);
    }

    public static DateTime Round(this DateTime dateTime, TimeSpan interval)
    {
        long halfIntervalTicks = (interval.Ticks + 1) >> 1;
        return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
    }
}

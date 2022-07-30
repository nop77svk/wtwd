namespace wtwd.Utilities;
using System.Globalization;

public static class TimeSpanExt
{
    public const string VariableStringSecondsFormat = @"ss";
    public const string VariableStringMinutesFormat = @"mm\:ss";
    public const string VariableStringHoursFormat = @"hh\:mm\:ss";
    public const string VariableStringDaysFormat = @"d\d\ hh\:mm\:ss";

    public static string ToVariableString(
        this TimeSpan input,
        string secondsFormat = VariableStringSecondsFormat,
        string minutesFormat = VariableStringMinutesFormat,
        string hoursFormat = VariableStringHoursFormat,
        string daysFormat = VariableStringDaysFormat
    )
    {
        string result;

        if (input >= TimeSpan.Zero)
        {
            if (input < TimeSpan.FromMinutes(1))
            {
                result = input.ToString(secondsFormat);
            }
            else if (input < TimeSpan.FromHours(1))
            {
                result = input.ToString(minutesFormat);
            }
            else if (input < TimeSpan.FromDays(1))
            {
                result = input.ToString(hoursFormat);
            }
            else
            {
                result = input.ToString(daysFormat);
            }
        }
        else
        {
            if (input > TimeSpan.FromMinutes(-1))
            {
                result = input.ToString(secondsFormat);
            }
            else if (input > TimeSpan.FromHours(-1))
            {
                result = input.ToString(minutesFormat);
            }
            else if (input > TimeSpan.FromDays(-1))
            {
                result = input.ToString(hoursFormat);
            }
            else
            {
                result = input.ToString(daysFormat);
            }
        }

        return result;
    }

    public static TimeSpan? Parse(string? timeSpanString)
    {
        TimeSpan? result = null;
        if (string.IsNullOrEmpty(timeSpanString))
            result = null;
        else if (timeSpanString.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            result = ParseWithSuffix(
                timeSpanString,
                x => x.TrimEnd('s', 'S'),
                x => TimeSpan.FromSeconds(x)
            );
        else if (timeSpanString.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            result = ParseWithSuffix(
                timeSpanString,
                x => x.TrimEnd('m', 'M'),
                x => TimeSpan.FromMinutes(x)
            );
        else if (timeSpanString.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            result = ParseWithSuffix(
                timeSpanString,
                x => x.TrimEnd('h', 'H'),
                x => TimeSpan.FromHours(x)
            );
        else if (timeSpanString.Contains(':'))
            result = ParseFromFormattedString(timeSpanString);
        else
            throw new ArgumentOutOfRangeException(nameof(timeSpanString), timeSpanString, "Don't know how to convert this input to a TimeSpan");

        return result;
    }

    private static TimeSpan? ParseFromFormattedString(string timeSpanString)
    {
        TimeSpan result;

        if (!TimeSpan.TryParseExact(timeSpanString, @"mm\:ss", CultureInfo.InvariantCulture, out result))
        {
            if (!TimeSpan.TryParseExact(timeSpanString, @"m\:ss", CultureInfo.InvariantCulture, out result))
            {
                if (!TimeSpan.TryParseExact(timeSpanString, @"HH\:mm\:ss", CultureInfo.InvariantCulture, out result))
                {
                    if (!TimeSpan.TryParseExact(timeSpanString, @"H\:mm\:ss", CultureInfo.InvariantCulture, out result))
                    {
                        if (!TimeSpan.TryParseExact(timeSpanString, @"HH\:mm", CultureInfo.InvariantCulture, out result))
                        {
                            if (!TimeSpan.TryParseExact(timeSpanString, @"H\:mm", CultureInfo.InvariantCulture, out result))
                            {
                                throw new ArgumentOutOfRangeException(nameof(timeSpanString), timeSpanString, "Don't know how to convert this input to a TimeSpan");
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private static TimeSpan? ParseWithSuffix(string timeSpanString, Func<string, string> trimmer, Func<float, TimeSpan> converter)
    {
        TimeSpan? result = null;

        string? trimmedInput = trimmer?.Invoke(timeSpanString);
        if (string.IsNullOrEmpty(trimmedInput))
        {
            result = null;
        }
        else
        {
            float inputAsNumber;
            if (!float.TryParse(trimmedInput, NumberStyles.Float, CultureInfo.CurrentCulture, out inputAsNumber))
            {
                if (!float.TryParse(trimmedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out inputAsNumber))
                {
                    throw new ArgumentOutOfRangeException(nameof(timeSpanString), $"Not a valid number ({trimmedInput})");
                }
            }

            result = converter(inputAsNumber);
        }

        return result;
    }
}

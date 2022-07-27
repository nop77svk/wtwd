namespace wtwd.utilities;

public class TimeSpanFromString
{
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
            result = TimeSpan.Parse(timeSpanString);
        else
            throw new ArgumentOutOfRangeException(nameof(timeSpanString), timeSpanString, "Don't know how to convert this input to a TimeSpan");

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
            try
            {
                inputAsNumber = float.Parse(trimmedInput);
            }
            catch (FormatException)
            {
                throw new ArgumentOutOfRangeException(nameof(timeSpanString), $"Not a valid number ({trimmedInput})");
            }
            catch (Exception e)
            {
                throw new ArgumentOutOfRangeException($"Failed to parse {trimmedInput} as float type", e);
            }

            result = converter(inputAsNumber);
        }

        return result;
    }
}

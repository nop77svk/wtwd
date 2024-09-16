#pragma warning disable SA1313
namespace NoP77svk.wtwd.cli.List;

using System;

using NoP77svk.wtwd.Utilities;

internal class TimeRecCsvDisplayPcSessionDto
{
    private readonly DateTime _checkIn;
    private readonly DateTime _checkOut;

    public TimeRecCsvDisplayPcSessionDto(DateTime checkIn, DateTime checkOut)
    {
        _checkIn = checkIn;
        _checkOut = checkOut;
    }

    [CsvHelper.Configuration.Attributes.Name("# DATE")]
    [CsvHelper.Configuration.Attributes.Index(0)]
    [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd")]
    public DateTime Date => _checkIn.Date;

    [CsvHelper.Configuration.Attributes.Name("CHECKIN")]
    [CsvHelper.Configuration.Attributes.Index(1)]
    [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd HH:mm:ss")]
    public DateTime CheckIn => _checkIn.Round(TimeSpan.FromMinutes(1));

    [CsvHelper.Configuration.Attributes.Name("CHECKOUT")]
    [CsvHelper.Configuration.Attributes.Index(2)]
    [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd HH:mm:ss")]
    public DateTime CheckOut => _checkOut.Round(TimeSpan.FromMinutes(1));

    [CsvHelper.Configuration.Attributes.Name("TASK-ID")]
    [CsvHelper.Configuration.Attributes.Index(3)]
    public string? TaskId { get; init; }

    [CsvHelper.Configuration.Attributes.Name("NOTES")]
    [CsvHelper.Configuration.Attributes.Index(4)]
    public string? Notes { get; init; }
}

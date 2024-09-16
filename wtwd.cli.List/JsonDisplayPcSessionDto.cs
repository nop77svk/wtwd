#pragma warning disable SA1313
namespace NoP77svk.wtwd.cli.List;

using System;

internal record JsonDisplayPcSessionDto(DateTime Start, DateTime End)
{
    public DateTime? FirstStart { get; init; }
    public IEnumerable<string?>? StartEventsOrdered { get; init; }
    public DateTime? LastEnd { get; init; }
    public IEnumerable<string?>? EndEventsOrdered { get; init; }
}

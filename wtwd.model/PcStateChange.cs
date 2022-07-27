#pragma warning disable SA1313, CA1416
namespace wtwd.model;

public record PcStateChange(PcStateChangeEvent Event, DateTime When)
{
}

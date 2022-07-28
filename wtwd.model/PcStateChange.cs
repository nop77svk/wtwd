#pragma warning disable SA1313, CA1416
namespace wtwd.Model;

public record PcStateChange(PcStateChangeEvent Event, DateTime When)
{
}

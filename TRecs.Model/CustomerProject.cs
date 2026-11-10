#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name
namespace TRecs.Model;

public class CustomerProject
{
    public CustomerProject(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }

    public decimal HourlyRate { get; init; } = 0.0m;
    public decimal TimeSummationFactor { get; init; } = 1.0m;
}

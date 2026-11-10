#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
namespace TRecs.Model;

public class WorkUnitTemplate
{
    public WorkUnitTemplate(int id, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name), "Name cannot be null nor empty.");
        }

        Id = id;
        Name = name;
    }

    public WorkUnitTemplate(int id, string name, CustomerProject customerProject)
        : this(id, name)
    {
        CustomerProject = customerProject;
    }

    public int Id { get; }
    public string Name { get; }
    public CustomerProject? CustomerProject { get; init; }
    public bool IsActive { get; init; } = true;
    public decimal? HourlyRate { get; init; }
    public decimal? TimeSummationFactor { get; init; }
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
namespace NoP77svk.TRecs.Model.TimeRecord;

public class TaskTemplate
{
    public TaskTemplate(int id, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name), "Name cannot be null nor empty.");
        }

        Id = id;
        Name = name;
    }

    public TaskTemplate(int id, string name, CustomerProject customerProject)
        : this(id, name)
    {
        CustomerProject = customerProject;
    }

    public int Id { get; }
    public string Name { get; set; }
    public CustomerProject? CustomerProject { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? HourlyRate { get; set; }
    public decimal? TimeSummationFactor { get; set; }
}

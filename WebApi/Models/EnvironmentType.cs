namespace WebApi.Models;

public class EnvironmentType
{
    public int EnvironmentTypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // public ICollection<WorkloadEnvironmentRegion> WorkloadEnvironmentRegions { get; set; } = new List<WorkloadEnvironmentRegion>();
}
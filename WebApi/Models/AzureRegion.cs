using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class AzureRegion
{
    [Key]
    public int RegionId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    
    // public ICollection<WorkloadEnvironmentRegion> WorkloadEnvironmentRegions { get; set; } = new List<WorkloadEnvironmentRegion>();
}
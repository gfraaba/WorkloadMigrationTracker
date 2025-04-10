using System.Text.Json.Serialization;

namespace WebApi.Models;

public class Workload
{
    public int WorkloadId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string AzureNamePrefix { get; set; }
    public string? PrimaryPOC { get; set; }
    public string? SecondaryPOC { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    [JsonIgnore] // Prevent circular reference
    public ICollection<WorkloadEnvironmentRegion> WorkloadEnvironmentRegions { get; set; } = new List<WorkloadEnvironmentRegion>();
}
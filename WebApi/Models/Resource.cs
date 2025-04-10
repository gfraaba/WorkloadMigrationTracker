using System.Text.Json.Serialization;

namespace WebApi.Models;

public class Resource
{
    public int ResourceId { get; set; }
    public required string Name { get; set; }
    
    // Foreign Keys
    public int WorkloadId { get; set; }
    public int WorkloadEnvironmentRegionId { get; set; }
    public int ResourceTypeId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation Properties
    [JsonIgnore] // Prevent circular reference
    public required Workload Workload { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public required WorkloadEnvironmentRegion WorkloadEnvironmentRegion { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public required ResourceType ResourceType { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public required ResourceStatus Status { get; set; }
}
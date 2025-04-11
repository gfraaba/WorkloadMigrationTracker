using System.Text.Json.Serialization;

namespace Shared.Models;

public class Resource
{
    public int ResourceId { get; set; }
    public required string Name { get; set; }
    
    // Foreign Keys
    public int WorkloadEnvironmentRegionId { get; set; }
    public int ResourceTypeId { get; set; }
    public required string Status { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public WorkloadEnvironmentRegion? WorkloadEnvironmentRegion { get; set; }
    [JsonIgnore]
    public ResourceType? ResourceType { get; set; }
}
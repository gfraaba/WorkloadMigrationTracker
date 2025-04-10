namespace Shared.Models;

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
    public required Workload Workload { get; set; }
    public required WorkloadEnvironmentRegion WorkloadEnvironmentRegion { get; set; }
    public required ResourceType ResourceType { get; set; }
    public required ResourceStatus Status { get; set; }
}
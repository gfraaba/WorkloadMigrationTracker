namespace Shared.DTOs;

public class ResourceDto
{
    public int ResourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WorkloadEnvironmentRegionId { get; set; }
    public int ResourceTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public ResourceTypeDto? ResourceType { get; set; }
    public WorkloadEnvironmentRegionDto? WorkloadEnvironmentRegion { get; set; }
}
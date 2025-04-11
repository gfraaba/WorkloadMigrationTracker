namespace Shared.DTOs;

public class WorkloadEnvironmentRegionDto
{
    public int WorkloadEnvironmentRegionId { get; set; }
    public string? AzureSubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }
    public int EnvironmentTypeId { get; set; }
    public int RegionId { get; set; }
    public int WorkloadId { get; set; }
    public string EnvironmentTypeName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string? Name { get; set; } // Added Name property
    public ICollection<ResourceDto> Resources { get; set; } = new List<ResourceDto>(); // Added Resources property
}
namespace Shared.DTOs;

public class WorkloadEnvironmentRegionDto
{
    public int WorkloadEnvironmentRegionId { get; set; }
    public string? AzureSubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }
    public int EnvironmentTypeId { get; set; }
    public int RegionId { get; set; }
    public string? EnvironmentTypeName { get; set; } // Flattened property
    public string? RegionName { get; set; } // Flattened property
}
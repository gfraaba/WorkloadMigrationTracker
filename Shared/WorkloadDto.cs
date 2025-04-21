namespace Shared.DTOs;

public class WorkloadDto
{
    public int WorkloadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AzureNamePrefix { get; set; } = string.Empty;
    public int LandingZonesCount { get; set; }
    public int ResourcesCount { get; set; }
    public string? PrimaryPOC { get; set; }
    public string? SecondaryPOC { get; set; }
    public List<WorkloadEnvironmentRegionDto> WorkloadEnvironmentRegions { get; set; } = new();
}
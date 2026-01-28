namespace Shared.DTOs;

using System.ComponentModel.DataAnnotations;

public class WorkloadEnvironmentRegionDto
{
    public int WorkloadEnvironmentRegionId { get; set; }

    public string? AzureSubscriptionId { get; set; }

    [Required(ErrorMessage = "Resource group name is required")]
    public string? ResourceGroupName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Environment selection is required")]
    public int EnvironmentTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Region selection is required")]
    public int RegionId { get; set; }

    public int WorkloadId { get; set; }

    public string EnvironmentTypeName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public ICollection<ResourceDto> Resources { get; set; } = new List<ResourceDto>();
}
namespace WebApi.Models;

public class WorkloadEnvironmentRegion
{
    public int WorkloadEnvironmentRegionId { get; set; }
    public string? AzureSubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }

    // Foreign Keys
    public int WorkloadId { get; set; }
    public int EnvironmentTypeId { get; set; }
    public int RegionId { get; set; }

    // Navigation Properties
    public Workload? Workload { get; set; }
    public EnvironmentType? EnvironmentType { get; set; }
    public AzureRegion? Region { get; set; }
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
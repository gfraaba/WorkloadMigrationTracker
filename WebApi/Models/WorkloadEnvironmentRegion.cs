using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
    [ValidateNever] 
    public required Workload Workload { get; set; }
    [ValidateNever] 
    public required EnvironmentType EnvironmentType { get; set; }
    [ValidateNever] 
    public required AzureRegion Region { get; set; }
    [JsonIgnore]
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
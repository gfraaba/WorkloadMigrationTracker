using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
    [ValidateNever]
    public required Workload Workload { get; set; }
    
    [ValidateNever]
    public required WorkloadEnvironmentRegion WorkloadEnvironmentRegion { get; set; }
    
    [ValidateNever]
    public required ResourceType ResourceType { get; set; }
    
    [ValidateNever]
    public required ResourceStatus Status { get; set; }
}
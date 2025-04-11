using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models;

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
    [ValidateNever]
    public WorkloadEnvironmentRegion? WorkloadEnvironmentRegion { get; set; }
    [ValidateNever]
    public ResourceType? ResourceType { get; set; }
}
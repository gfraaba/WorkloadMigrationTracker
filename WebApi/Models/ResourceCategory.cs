using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models;

public class ResourceCategory
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public string? AzureServiceType { get; set; }

    [ValidateNever]
    [JsonIgnore]
    public ICollection<ResourceType> ResourceTypes { get; set; } = new List<ResourceType>();
}
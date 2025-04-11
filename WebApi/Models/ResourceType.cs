using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models;

public class ResourceType
{
    public int TypeId { get; set; }
    public required string Name { get; set; }
    public required string AzureResourceType { get; set; }
    public int CategoryId { get; set; }

    [ValidateNever]
    public ResourceCategory? Category { get; set; } // Removed default initialization
    [JsonIgnore]
    [ValidateNever]
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
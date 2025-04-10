using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApi.Models;

public class ResourceCategory
{
    [Key]
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public string? AzureServiceType { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public ICollection<ResourceType> ResourceTypes { get; set; } = new List<ResourceType>();
}
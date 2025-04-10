using System.Text.Json.Serialization;

namespace Shared.Models;

public class ResourceType
{
    public int TypeId { get; set; }
    public required string Name { get; set; }
    public required string AzureResourceType { get; set; }
    public int CategoryId { get; set; }
    public ResourceCategory Category { get; set; } = null!;
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}

public class ResourceCategory
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public string? AzureServiceType { get; set; }

    [JsonIgnore] // Prevent circular reference
    public ICollection<ResourceType> ResourceTypes { get; set; } = new List<ResourceType>();
}
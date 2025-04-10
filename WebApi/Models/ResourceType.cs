using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApi.Models;

public class ResourceType
{
    [Key]
    public int TypeId { get; set; }
    public required string Name { get; set; }
    public required string AzureResourceType { get; set; }
    
    // Foreign Key
    public int CategoryId { get; set; }
    
    // Navigation Properties
    public ResourceCategory Category { get; set; } = null!; // For seeding
    [JsonIgnore] // Prevent circular reference
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
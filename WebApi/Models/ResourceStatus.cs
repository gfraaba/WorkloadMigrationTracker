using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApi.Models;

public class ResourceStatus
{
    [Key]
    public int StatusId { get; set; }
    public required string Name { get; set; }
    
    [JsonIgnore] // Prevent circular reference
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
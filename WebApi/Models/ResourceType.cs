using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; 

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
    
    // [ValidateNever]: Prevents the model validation framework from validating this property. This is useful when you want to skip validation for a specific property.
    [ValidateNever] // Prevent model validation
    public ResourceCategory Category { get; set; } = null!; // For seeding
    
    // [JsonIgnore]: Prevents the Category property from being serialized or deserialized in JSON. This ensures that the property is not included in the request or response payload.
    [JsonIgnore] // Prevent circular reference
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
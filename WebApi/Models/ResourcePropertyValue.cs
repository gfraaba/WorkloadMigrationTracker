namespace WebApi.Models;

public class ResourcePropertyValue
{
    public int PropertyValueId { get; set; } // Primary key
    public int ResourceId { get; set; } // Foreign key to Resource
    public int PropertyId { get; set; } // Foreign key to ResourceProperty
    public string Value { get; set; } = string.Empty; // Value stored as a string for flexibility
}
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models;

public class ResourceProperty
{
    public int PropertyId { get; set; } // Primary key
    public int ResourceTypeId { get; set; } // Foreign key to ResourceType
    public string Name { get; set; } = string.Empty; // Name of the property (e.g., "OsType", "VmSize")
    public string DataType { get; set; } = string.Empty; // Data type of the property (e.g., "string", "int")
    public bool IsRequired { get; set; } // Whether the property is required
    public string? DefaultValue { get; set; } // Default value for the property (optional)
}
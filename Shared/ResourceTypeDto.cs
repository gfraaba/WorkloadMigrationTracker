namespace Shared.DTOs;

public class ResourceTypeDto
{
    public int TypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AzureResourceType { get; set; } = string.Empty;
    public int? CategoryId { get; set; } // Ensure nullable to handle cases where Category is null
    public ResourceCategoryDto? Category { get; set; }
}
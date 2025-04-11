namespace Shared.DTOs;

public class ResourceCategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AzureServiceType { get; set; }
    public List<ResourceTypeDto>? ResourceTypes { get; set; }
}
namespace Shared.DTOs;

public class ResourcePropertyDto
{
    public int PropertyId { get; set; }
    public int ResourceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
}
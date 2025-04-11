namespace Shared.DTOs;

public class AzureRegionDto
{
    public int RegionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
namespace Shared.Models;

public class AzureRegion
{
    public int RegionId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
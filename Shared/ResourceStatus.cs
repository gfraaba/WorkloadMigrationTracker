namespace Shared.Models;

public class ResourceStatus
{
    public int StatusId { get; set; }
    public required string Name { get; set; }
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
namespace WebApi.Models;

public class EnvironmentType
{
    public int EnvironmentTypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
namespace Shared.DTOs;

public class EnvironmentRegionsDto
{
    public string EnvironmentName { get; set; } = string.Empty;
    public List<AzureRegionDto> Regions { get; set; } = new();
}
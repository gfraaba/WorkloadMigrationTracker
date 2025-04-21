using System.Net.Http.Json;
using Shared.DTOs;

namespace WebApp.Services;

public class WorkloadService
{
    private readonly HttpClient _httpClient;

    public WorkloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<WorkloadDto>> GetWorkloadsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching workloads from API.");
        try
        {
            var workloads = await _httpClient.GetFromJsonAsync<List<WorkloadDto>>("api/workloads") ?? new List<WorkloadDto>();
            Console.WriteLine("WorkloadService: Workloads fetched successfully.");
            foreach (var workload in workloads)
            {
                Console.WriteLine($"WorkloadService: WorkloadId={workload.WorkloadId}, Name={workload.Name}, LandingZonesCount={workload.LandingZonesCount}");
            }
            return workloads ?? new List<WorkloadDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error fetching workloads - {ex.Message}");
            throw;
        }
    }

    public async Task CreateWorkloadAsync(WorkloadDto workload)
    {
        var response = await _httpClient.PostAsJsonAsync("api/workloads", workload);
        response.EnsureSuccessStatusCode();
    }

    public async Task<WorkloadDto> GetWorkloadByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<WorkloadDto>($"api/workloads/{id}");
    }

    public async Task UpdateWorkloadAsync(WorkloadDto workload)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/workloads/{workload.WorkloadId}", workload);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteWorkloadAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"workloads/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task AddResourceToWorkloadAsync(int workloadEnvironmentRegionId, ResourceDto resource)
    {
        Console.WriteLine($"WorkloadService: Adding resource to LZ Id {workloadEnvironmentRegionId}.");
        Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(resource)}");
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/resources/add-to-workload/{workloadEnvironmentRegionId}", resource); 
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Resource added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error adding resource - {ex.Message}");
            throw;
        }
    }

    public async Task AddResourceAsync(ResourceDto resource)
    {
        Console.WriteLine($"WorkloadService: Adding resource to LZ Id {resource.WorkloadEnvironmentRegionId}.");
        Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(resource)}");
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/resources", resource);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Resource added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error adding resource - {ex.Message}");
            throw;
        }
    }

    public async Task<List<EnvironmentTypeDto>> GetEnvironmentsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching environments from API.");
        return await _httpClient.GetFromJsonAsync<List<EnvironmentTypeDto>>("api/environmenttypes/environments") ?? new List<EnvironmentTypeDto>();
    }

    public async Task<List<AzureRegionDto>> GetRegionsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching regions from API.");
        return await _httpClient.GetFromJsonAsync<List<AzureRegionDto>>("api/azureregions/regions") ?? new List<AzureRegionDto>();
    }

    public async Task<List<ResourceTypeDto>> GetResourceTypesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching resource types from API.");
        return await _httpClient.GetFromJsonAsync<List<ResourceTypeDto>>("api/resourcetypes") ?? new List<ResourceTypeDto>();
    }

    public async Task<List<WorkloadEnvironmentRegionDto>> GetLandingZonesForWorkloadAsync(int workloadId)
    {
        Console.WriteLine($"WorkloadService: Fetching landing zones for workload {workloadId}.");
        return await _httpClient.GetFromJsonAsync<List<WorkloadEnvironmentRegionDto>>($"api/workloadenvironmentregions/workload/{workloadId}") ?? new List<WorkloadEnvironmentRegionDto>();
    }

    public async Task<List<ResourceStatusDto>> GetStatusesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching statuses from API.");
        return await _httpClient.GetFromJsonAsync<List<ResourceStatusDto>>("api/resourcestatuses") ?? new List<ResourceStatusDto>();
    }

    public async Task AddLandingZoneAsync(WorkloadEnvironmentRegionDto landingZone)
    {
        Console.WriteLine("WorkloadService: Adding a new landing zone.");
        var response = await _httpClient.PostAsJsonAsync("api/workloadenvironmentregions", landingZone);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("WorkloadService: Landing zone added successfully.");
    }

    public async Task<List<ResourceDto>> GetResourcesForLandingZoneAsync(int landingZoneId)
    {
        Console.WriteLine($"WorkloadService: Fetching resources for landing zone {landingZoneId}.");
        return await _httpClient.GetFromJsonAsync<List<ResourceDto>>($"api/resources/landing-zone/{landingZoneId}") ?? new List<ResourceDto>();
    }

    public async Task<List<ResourceDto>> GetResourcesAsync(int workloadEnvironmentRegionId)
    {
        Console.WriteLine($"Fetching resources for workload environment region ID: {workloadEnvironmentRegionId}");
        return await _httpClient.GetFromJsonAsync<List<ResourceDto>>($"api/resources/landing-zone/{workloadEnvironmentRegionId}") ?? new List<ResourceDto>();
    }

    public async Task<List<ResourcePropertyDto>> GetResourcePropertiesAsync(int resourceTypeId)
    {
        Console.WriteLine($"Fetching resource properties for ResourceTypeId: {resourceTypeId}");
        return await _httpClient.GetFromJsonAsync<List<ResourcePropertyDto>>($"api/resourceproperties/{resourceTypeId}") 
            ?? new List<ResourcePropertyDto>();
    }
}
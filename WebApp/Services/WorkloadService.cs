using System.Net.Http.Json;
using Shared.Models;
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

    public async Task CreateWorkloadAsync(Workload workload)
    {
        var response = await _httpClient.PostAsJsonAsync("workloads", workload);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Workload> GetWorkloadByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Workload>($"workloads/{id}");
    }

    public async Task UpdateWorkloadAsync(Workload workload)
    {
        var response = await _httpClient.PutAsJsonAsync($"workloads/{workload.WorkloadId}", workload);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteWorkloadAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"workloads/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task AddResourceToWorkloadAsync(int workloadEnvironmentRegionId, Resource resource)
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

    public async Task<List<EnvironmentType>> GetEnvironmentsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching environments from API.");
        return await _httpClient.GetFromJsonAsync<List<EnvironmentType>>("api/environmenttypes/environments") ?? new List<EnvironmentType>(); // Added 'api/' prefix to match the EnvironmentTypesController route
    }

    public async Task<List<AzureRegion>> GetRegionsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching regions from API.");
        return await _httpClient.GetFromJsonAsync<List<AzureRegion>>("api/azureregions/regions") ?? new List<AzureRegion>(); // Added 'api/' prefix to match the AzureRegionsController route
    }

    public async Task<List<ResourceType>> GetResourceTypesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching resource types from API.");
        return await _httpClient.GetFromJsonAsync<List<ResourceType>>("api/resourcetypes") ?? new List<ResourceType>(); // Added 'api/' prefix to match the ResourceTypesController route
    }

    public async Task<List<WorkloadEnvironmentRegion>> GetLandingZonesForWorkloadAsync(int workloadId)
    {
        Console.WriteLine($"WorkloadService: Fetching landing zones for workload {workloadId}.");
        return await _httpClient.GetFromJsonAsync<List<WorkloadEnvironmentRegion>>($"api/workloadenvironmentregions/workload/{workloadId}") ?? new List<WorkloadEnvironmentRegion>();
    }

    public async Task<List<ResourceStatus>> GetStatusesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching statuses from API.");
        return await _httpClient.GetFromJsonAsync<List<ResourceStatus>>("api/resourcestatuses") ?? new List<ResourceStatus>();
    }

    public async Task AddLandingZoneAsync(WorkloadEnvironmentRegion landingZone)
    {
        Console.WriteLine("WorkloadService: Adding a new landing zone.");
        var response = await _httpClient.PostAsJsonAsync("api/workloadenvironmentregions", landingZone);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("WorkloadService: Landing zone added successfully.");
    }
}
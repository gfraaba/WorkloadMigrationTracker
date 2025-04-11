using System.Net.Http.Json;
using Shared.Models; // Updated namespace for Workload model

namespace WebApp.Services;

public class WorkloadService
{
    private readonly HttpClient _httpClient;

    public WorkloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Workload>> GetWorkloadsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching workloads from API.");
        try
        {
            var workloads = await _httpClient.GetFromJsonAsync<List<Workload>>("api/workloads"); // Added 'api/' prefix to match the WorkloadsController route
            Console.WriteLine("WorkloadService: Workloads fetched successfully.");
            return workloads ?? new List<Workload>();
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

    public async Task AddResourceToWorkloadAsync(int workloadId, Resource resource)
    {
        Console.WriteLine($"WorkloadService: Adding resource to workload {workloadId}.");
        Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(resource)}");
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/resources/add-to-workload/{workloadId}", resource); // Added 'api/' prefix to match the ResourcesController route
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
}
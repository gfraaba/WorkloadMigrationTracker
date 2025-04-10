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
            var workloads = await _httpClient.GetFromJsonAsync<List<Workload>>("workloads");
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
        var response = await _httpClient.PostAsJsonAsync($"resources/add-to-workload/{workloadId}", resource);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("WorkloadService: Resource added successfully.");
    }
}
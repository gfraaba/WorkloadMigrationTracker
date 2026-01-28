using System.Net.Http.Json;
using Microsoft.JSInterop;
using Shared.DTOs;

namespace WebApp.Services;

public class WorkloadService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private async Task<System.IO.Stream> GetStreamWithFallbackAsync(string url)
    {
        var resp = await _httpClient.GetAsync(url);
        if (resp.IsSuccessStatusCode)
        {
            var contentType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (!contentType.Contains("text/html"))
            {
                return await resp.Content.ReadAsStreamAsync();
            }
        }
        try
        {
            var testBase = await _jsRuntime.InvokeAsync<string>("getTestApiBaseUrl");
            if (!string.IsNullOrEmpty(testBase))
            {
                using var client = new HttpClient { BaseAddress = new Uri(testBase) };
                var fallback = await client.GetAsync(url);
                fallback.EnsureSuccessStatusCode();
                return await fallback.Content.ReadAsStreamAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: GetStreamWithFallbackAsync failed - {ex.Message}");
        }

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync();
    }

    private async Task<HttpResponseMessage> SendWithFallbackAsync(Func<HttpClient, Task<HttpResponseMessage>> send)
    {
        var resp = await send(_httpClient);
        var contentType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (resp.IsSuccessStatusCode && !contentType.Contains("text/html"))
            return resp;

        try
        {
            var testBase = await _jsRuntime.InvokeAsync<string>("getTestApiBaseUrl");
            if (!string.IsNullOrEmpty(testBase))
            {
                using var client = new HttpClient { BaseAddress = new Uri(testBase) };
                var fallback = await send(client);
                return fallback;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: SendWithFallbackAsync failed - {ex.Message}");
        }

        return resp;
    }

    public WorkloadService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }
    
    public async Task<List<WorkloadDto>> GetWorkloadsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching workloads from API.");
        try
        {
            var resp = await _httpClient.GetAsync("api/workloads");
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"WorkloadService: Non-success status {resp.StatusCode} while fetching workloads.");
                return new List<WorkloadDto>();
            }

            // Handle empty responses gracefully
            var contentLength = resp.Content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value == 0)
            {
                Console.WriteLine("WorkloadService: Workloads endpoint returned empty body.");
                return new List<WorkloadDto>();
            }

            var contentType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var stream = await resp.Content.ReadAsStreamAsync();

            // If the SPA host served HTML (text/html) instead of the API JSON, try fallback to TEST_API_BASE_URL
            if (contentType.Contains("text/html") )
            {
                try
                {
                    var testBase = await _jsRuntime.InvokeAsync<string>("getTestApiBaseUrl");
                    if (!string.IsNullOrEmpty(testBase))
                    {
                        Console.WriteLine($"WorkloadService: Detected HTML response; falling back to TEST_API_BASE_URL={testBase}");
                        using var client = new HttpClient { BaseAddress = new Uri(testBase) };
                        var fallback = await client.GetAsync("api/workloads");
                        if (fallback.IsSuccessStatusCode)
                        {
                            stream = await fallback.Content.ReadAsStreamAsync();
                        }
                    }
                }
                catch (Exception jex)
                {
                    Console.WriteLine($"WorkloadService: Fallback fetch failed - {jex.Message}");
                }
            }
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var workloads = await System.Text.Json.JsonSerializer.DeserializeAsync<List<WorkloadDto>>(stream, options)
                                ?? new List<WorkloadDto>();
                Console.WriteLine("WorkloadService: Workloads fetched successfully.");
                foreach (var workload in workloads)
                {
                    Console.WriteLine($"WorkloadService: WorkloadId={workload.WorkloadId}, Name={workload.Name}, LandingZonesCount={workload.LandingZonesCount}");
                }
                return workloads;
            }
            catch (System.Text.Json.JsonException jex)
            {
                Console.WriteLine($"WorkloadService: JSON parse error - {jex.Message}");
                return new List<WorkloadDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error fetching workloads - {ex.Message}");
            return new List<WorkloadDto>();
        }
    }

    public async Task AddWorkloadAsync(WorkloadDto workload)
    {
        var response = await SendWithFallbackAsync(client => client.PostAsJsonAsync("api/workloads", workload));
        response.EnsureSuccessStatusCode();
    }

    public async Task<WorkloadDto> GetWorkloadByIdAsync(int id)
    {
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/workloads/{id}");
            var dto = await System.Text.Json.JsonSerializer.DeserializeAsync<WorkloadDto>(stream, _jsonOptions);
            return dto ?? new WorkloadDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetWorkloadByIdAsync - {ex.Message}");
            return new WorkloadDto();
        }
    }

    public async Task UpdateWorkloadAsync(WorkloadDto workload)
    {
        Console.WriteLine($"WorkloadService: Updating workload with ID {workload.WorkloadId}.");
        try
        {
            var response = await SendWithFallbackAsync(client => client.PutAsJsonAsync($"api/workloads/{workload.WorkloadId}", workload));
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Workload updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error updating workload - {ex.Message}");
            throw;
        }
    }

    public async Task DeleteWorkloadAsync(int workloadId)
    {
        Console.WriteLine($"WorkloadService: Deleting workload with ID {workloadId}.");
        try
        {
            var response = await SendWithFallbackAsync(client => client.DeleteAsync($"api/workloads/{workloadId}"));
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Workload deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error deleting workload - {ex.Message}");
            throw;
        }
    }

    public async Task AddResourceToWorkloadAsync(int workloadEnvironmentRegionId, ResourceDto resource)
    {
        Console.WriteLine($"WorkloadService: Adding resource to LZ Id {workloadEnvironmentRegionId}.");
        Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(resource)}");
        try
        {
            var response = await SendWithFallbackAsync(client => client.PostAsJsonAsync($"api/resources/add-to-workload/{workloadEnvironmentRegionId}", resource));
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
            var response = await SendWithFallbackAsync(client => client.PostAsJsonAsync("api/resources", resource));
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
        try
        {
            var stream = await GetStreamWithFallbackAsync("api/environmenttypes/environments");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<EnvironmentTypeDto>>(stream, _jsonOptions) ?? new List<EnvironmentTypeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetEnvironmentsAsync - {ex.Message}");
            return new List<EnvironmentTypeDto>();
        }
    }

    public async Task<List<AzureRegionDto>> GetRegionsAsync()
    {
        Console.WriteLine("WorkloadService: Fetching regions from API.");
        try
        {
            var stream = await GetStreamWithFallbackAsync("api/azureregions/regions");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<AzureRegionDto>>(stream, _jsonOptions) ?? new List<AzureRegionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetRegionsAsync - {ex.Message}");
            return new List<AzureRegionDto>();
        }
    }

    public async Task<List<ResourceTypeDto>> GetResourceTypesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching resource types from API.");
        try
        {
            var stream = await GetStreamWithFallbackAsync("api/resourcetypes");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<ResourceTypeDto>>(stream, _jsonOptions) ?? new List<ResourceTypeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetResourceTypesAsync - {ex.Message}");
            return new List<ResourceTypeDto>();
        }
    }

    public async Task<List<WorkloadEnvironmentRegionDto>> GetLandingZonesForWorkloadAsync(int workloadId)
    {
        Console.WriteLine($"WorkloadService: Fetching landing zones for workload {workloadId}.");
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/workloadenvironmentregions/workload/{workloadId}");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<WorkloadEnvironmentRegionDto>>(stream, _jsonOptions) ?? new List<WorkloadEnvironmentRegionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetLandingZonesForWorkloadAsync - {ex.Message}");
            return new List<WorkloadEnvironmentRegionDto>();
        }
    }

    public async Task<List<ResourceStatusDto>> GetStatusesAsync()
    {
        Console.WriteLine("WorkloadService: Fetching statuses from API.");
        try
        {
            var stream = await GetStreamWithFallbackAsync("api/resourcestatuses");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<ResourceStatusDto>>(stream, _jsonOptions) ?? new List<ResourceStatusDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetStatusesAsync - {ex.Message}");
            return new List<ResourceStatusDto>();
        }
    }

    public async Task AddLandingZoneAsync(WorkloadEnvironmentRegionDto landingZone)
    {
        try
        {
            Console.WriteLine("AddLandingZoneAsync: Sending the following payload to the server:");
            Console.WriteLine($"WorkloadId: {landingZone.WorkloadId}");
            Console.WriteLine($"EnvironmentTypeId: {landingZone.EnvironmentTypeId}");
            Console.WriteLine($"RegionId: {landingZone.RegionId}");
            Console.WriteLine($"AzureSubscriptionId: {landingZone.AzureSubscriptionId}");
            Console.WriteLine($"ResourceGroupName: {landingZone.ResourceGroupName}");

            var response = await SendWithFallbackAsync(client => client.PostAsJsonAsync("api/workloadenvironmentregions", landingZone));
            response.EnsureSuccessStatusCode();

            Console.WriteLine("AddLandingZoneAsync: Landing zone added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddLandingZoneAsync: Error occurred while adding landing zone - {ex.Message}");
            throw;
        }
    }

    public async Task<WorkloadEnvironmentRegionDto?> GetLandingZoneAsync(int landingZoneId)
    {
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/WorkloadEnvironmentRegions/{landingZoneId}");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<WorkloadEnvironmentRegionDto>(stream, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetLandingZoneAsync - {ex.Message}");
            return null;
        }
    }

    public async Task<List<ResourceDto>> GetResourcesForLandingZoneAsync(int landingZoneId)
    {
        Console.WriteLine($"WorkloadService: Fetching resources for landing zone {landingZoneId}.");
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/resources/landing-zone/{landingZoneId}");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<ResourceDto>>(stream, _jsonOptions) ?? new List<ResourceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetResourcesForLandingZoneAsync - {ex.Message}");
            return new List<ResourceDto>();
        }
    }

    public async Task<List<ResourceDto>> GetResourcesAsync(int workloadEnvironmentRegionId)
    {
        Console.WriteLine($"Fetching resources for workload environment region ID: {workloadEnvironmentRegionId}");
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/resources/landing-zone/{workloadEnvironmentRegionId}");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<ResourceDto>>(stream, _jsonOptions) ?? new List<ResourceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetResourcesAsync - {ex.Message}");
            return new List<ResourceDto>();
        }
    }

    public async Task<List<ResourcePropertyDto>> GetResourcePropertiesAsync(int resourceTypeId)
    {
        Console.WriteLine($"Fetching resource properties for ResourceTypeId: {resourceTypeId}");
        try
        {
            var stream = await GetStreamWithFallbackAsync($"api/resourceproperties/{resourceTypeId}");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<List<ResourcePropertyDto>>(stream, _jsonOptions) ?? new List<ResourcePropertyDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error GetResourcePropertiesAsync - {ex.Message}");
            return new List<ResourcePropertyDto>();
        }
    }

    public async Task<Dictionary<int, EnvironmentRegionsDto>> GetAvailableEnvironmentsAndRegionsAsync(int workloadId)
    {
        Console.WriteLine($"WorkloadService: Fetching available environments and regions for workload {workloadId}.");
        try
        {
            var url = $"api/WorkloadEnvironmentRegions/available-environments-and-regions/{workloadId}";
            var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"WorkloadService: Non-success status {resp.StatusCode} while fetching available environments.");
                return new Dictionary<int, EnvironmentRegionsDto>();
            }

            var contentType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var stream = await resp.Content.ReadAsStreamAsync();

            if (contentType.Contains("text/html"))
            {
                try
                {
                    var testBase = await _jsRuntime.InvokeAsync<string>("getTestApiBaseUrl");
                    if (!string.IsNullOrEmpty(testBase))
                    {
                        Console.WriteLine($"WorkloadService: Detected HTML response; falling back to TEST_API_BASE_URL={testBase}");
                        using var client = new System.Net.Http.HttpClient { BaseAddress = new Uri(testBase) };
                        var fallback = await client.GetAsync(url);
                        if (fallback.IsSuccessStatusCode)
                        {
                            stream = await fallback.Content.ReadAsStreamAsync();
                        }
                    }
                }
                catch (Exception jex)
                {
                    Console.WriteLine($"WorkloadService: Fallback fetch failed - {jex.Message}");
                }
            }

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var mapping = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<int, EnvironmentRegionsDto>>(stream, options)
                          ?? new Dictionary<int, EnvironmentRegionsDto>();
            return mapping;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error fetching available environments and regions - {ex.Message}");
            return new Dictionary<int, EnvironmentRegionsDto>();
        }
    }

    public async Task UpdateLandingZoneAsync(WorkloadEnvironmentRegionDto landingZone)
    {
        Console.WriteLine($"WorkloadService: Updating landing zone with ID {landingZone.WorkloadEnvironmentRegionId}.");
        try
        {
            var response = await SendWithFallbackAsync(client => client.PutAsJsonAsync($"api/WorkloadEnvironmentRegions/{landingZone.WorkloadEnvironmentRegionId}", landingZone));
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Landing zone updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error updating landing zone - {ex.Message}");
            throw;
        }
    }

    public async Task DeleteLandingZoneAsync(int landingZoneId)
    {
        Console.WriteLine($"WorkloadService: Deleting landing zone with ID {landingZoneId}.");
        try
        {
            var response = await SendWithFallbackAsync(client => client.DeleteAsync($"api/WorkloadEnvironmentRegions/{landingZoneId}"));
            response.EnsureSuccessStatusCode();
            Console.WriteLine("WorkloadService: Landing zone deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkloadService: Error deleting landing zone - {ex.Message}");
            throw;
        }
    }
}
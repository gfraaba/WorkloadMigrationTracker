using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class WorkloadEnvironmentRegionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkloadEnvironmentRegionsController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<WorkloadEnvironmentRegion> IncludeRelatedEntities()
    {
        return _context.WorkloadEnvironmentRegions
            .Include(w => w.Workload)
            .Include(w => w.EnvironmentType)
            .Include(w => w.Region)
            .Include(w => w.Resources)
            .ThenInclude(r => r.ResourceType);
    }

    private WorkloadEnvironmentRegionDto MapToDto(WorkloadEnvironmentRegion workloadEnvironmentRegion)
    {
        return new WorkloadEnvironmentRegionDto
        {
            WorkloadEnvironmentRegionId = workloadEnvironmentRegion.WorkloadEnvironmentRegionId,
            AzureSubscriptionId = workloadEnvironmentRegion.AzureSubscriptionId,
            ResourceGroupName = workloadEnvironmentRegion.ResourceGroupName,
            Name = workloadEnvironmentRegion.ResourceGroupName,
            EnvironmentTypeId = workloadEnvironmentRegion.EnvironmentTypeId,
            RegionId = workloadEnvironmentRegion.RegionId,
            WorkloadId = workloadEnvironmentRegion.WorkloadId,
            EnvironmentTypeName = workloadEnvironmentRegion.EnvironmentType?.Name ?? string.Empty,
            RegionName = workloadEnvironmentRegion.Region?.Name ?? string.Empty,
            Resources = workloadEnvironmentRegion.Resources.Select(r => new ResourceDto
            {
                ResourceId = r.ResourceId,
                Name = r.Name,
                WorkloadEnvironmentRegionId = r.WorkloadEnvironmentRegionId,
                ResourceTypeId = r.ResourceTypeId,
                Status = r.Status,
                ResourceType = r.ResourceType != null ? new ResourceTypeDto
                {
                    TypeId = r.ResourceType.TypeId,
                    Name = r.ResourceType.Name,
                    AzureResourceType = r.ResourceType.AzureResourceType,
                    CategoryId = r.ResourceType.CategoryId,
                    Category = r.ResourceType.Category != null ? new ResourceCategoryDto
                    {
                        CategoryId = r.ResourceType.Category.CategoryId,
                        Name = r.ResourceType.Category.Name
                    } : null
                } : null
            }).ToList()
        };
    }

    // GET: api/WorkloadEnvironmentRegions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkloadEnvironmentRegionDto>>> GetWorkloadEnvironmentRegions()
    {
        var workloadEnvironmentRegions = await IncludeRelatedEntities().ToListAsync();
        var workloadEnvironmentRegionDtos = workloadEnvironmentRegions.Select(MapToDto);
        return Ok(workloadEnvironmentRegionDtos);
    }

    // GET: api/WorkloadEnvironmentRegions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkloadEnvironmentRegionDto>> GetWorkloadEnvironmentRegion(int id)
    {
        var workloadEnvironmentRegion = await IncludeRelatedEntities()
            .FirstOrDefaultAsync(w => w.WorkloadEnvironmentRegionId == id);

        if (workloadEnvironmentRegion == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(workloadEnvironmentRegion));
    }

    // GET: api/WorkloadEnvironmentRegions/workload/{workloadId}
    [HttpGet("workload/{workloadId}")]
    public async Task<ActionResult<IEnumerable<WorkloadEnvironmentRegionDto>>> GetLandingZonesForWorkload(int workloadId)
    {
        var landingZones = await IncludeRelatedEntities()
            .Where(w => w.WorkloadId == workloadId)
            .ToListAsync();

        if (!landingZones.Any())
        {
            return Ok(new List<WorkloadEnvironmentRegionDto>()); // Return an empty list with a 200 status code
        }

        var landingZoneDtos = landingZones.Select(MapToDto);
        return Ok(landingZoneDtos);
    }

    [HttpGet("available-environments-and-regions/{workloadId}")]
    public async Task<ActionResult<Dictionary<int, EnvironmentRegionsDto>>> GetAvailableEnvironmentsAndRegions(int workloadId)
    {
        try
        {
            Console.WriteLine($"Fetching available environments and regions for workload {workloadId}.");

            // Fetch all existing landing zones for the workload
            var existingLandingZones = await _context.WorkloadEnvironmentRegions
                .Where(wr => wr.WorkloadId == workloadId)
                .ToListAsync();
            Console.WriteLine($"Existing landing zones count: {existingLandingZones.Count}");

            // Get all environments and regions
            var allEnvironments = await _context.EnvironmentTypes.ToListAsync();
            var allRegions = await _context.AzureRegions.ToListAsync();
            Console.WriteLine($"Total environments: {allEnvironments.Count}, Total regions: {allRegions.Count}");

            // Filter out used environment-region combinations
            var usedEnvironmentRegionPairs = existingLandingZones
                .Select(lz => new { lz.EnvironmentTypeId, lz.RegionId })
                .ToHashSet();
            Console.WriteLine($"Used environment-region pairs count: {usedEnvironmentRegionPairs.Count}");

            // Build the mapping of available environments and regions
            var environmentRegionMapping = new Dictionary<int, EnvironmentRegionsDto>();

            foreach (var environment in allEnvironments)
            {
                Console.WriteLine($"Processing environment: {environment.Name} (ID: {environment.EnvironmentTypeId})");

                var availableRegions = allRegions
                    .Where(region => !usedEnvironmentRegionPairs.Any(pair =>
                        pair.EnvironmentTypeId == environment.EnvironmentTypeId &&
                        pair.RegionId == region.RegionId))
                    .Select(region => new AzureRegionDto
                    {
                        RegionId = region.RegionId,
                        Name = region.Name,
                        Code = region.Code,
                        IsActive = region.IsActive
                    })
                    .ToList();

                Console.WriteLine($"Available regions for environment {environment.Name}: {availableRegions.Count}");

                // Only add environments with available regions
                if (availableRegions.Any())
                {
                    environmentRegionMapping[environment.EnvironmentTypeId] = new EnvironmentRegionsDto
                    {
                        EnvironmentName = environment.Name,
                        Regions = availableRegions
                    };
                    Console.WriteLine($"Added environment: {environment.Name} with {availableRegions.Count} regions.");
                }
            }

            Console.WriteLine($"Total available environments: {environmentRegionMapping.Count}");
            return Ok(environmentRegionMapping);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAvailableEnvironmentsAndRegions: {ex.Message}");
            return StatusCode(500, "An error occurred while fetching available environments and regions.");
        }
    }

    // POST: api/WorkloadEnvironmentRegions
    [HttpPost]
    public async Task<ActionResult<WorkloadEnvironmentRegion>> PostWorkloadEnvironmentRegion(WorkloadEnvironmentRegion workloadEnvironmentRegion)
    {
        try
        {
            Console.WriteLine("PostWorkloadEnvironmentRegion: Received the following payload:");
            Console.WriteLine($"WorkloadId: {workloadEnvironmentRegion.WorkloadId}");
            Console.WriteLine($"EnvironmentTypeId: {workloadEnvironmentRegion.EnvironmentTypeId}");
            Console.WriteLine($"RegionId: {workloadEnvironmentRegion.RegionId}");
            Console.WriteLine($"AzureSubscriptionId: {workloadEnvironmentRegion.AzureSubscriptionId}");
            Console.WriteLine($"ResourceGroupName: {workloadEnvironmentRegion.ResourceGroupName}");

            // Ignore navigation properties during validation
            ModelState.Remove(nameof(WorkloadEnvironmentRegion.Workload));
            ModelState.Remove(nameof(WorkloadEnvironmentRegion.EnvironmentType));
            ModelState.Remove(nameof(WorkloadEnvironmentRegion.Region));

            // Validate required fields
            if (workloadEnvironmentRegion.WorkloadId <= 0)
            {
                return BadRequest(new { error = "WorkloadId is required and must be greater than 0." });
            }
            if (workloadEnvironmentRegion.EnvironmentTypeId <= 0)
            {
                return BadRequest(new { error = "EnvironmentTypeId is required and must be greater than 0." });
            }
            if (workloadEnvironmentRegion.RegionId <= 0)
            {
                return BadRequest(new { error = "RegionId is required and must be greater than 0." });
            }
            if (string.IsNullOrWhiteSpace(workloadEnvironmentRegion.ResourceGroupName))
            {
                return BadRequest(new { error = "ResourceGroupName is required and cannot be empty." });
            }

            // Check for duplicate Environment-Region combination
            var exists = await _context.WorkloadEnvironmentRegions.AnyAsync(wr =>
                wr.WorkloadId == workloadEnvironmentRegion.WorkloadId &&
                wr.EnvironmentTypeId == workloadEnvironmentRegion.EnvironmentTypeId &&
                wr.RegionId == workloadEnvironmentRegion.RegionId);

            if (exists)
            {
                Console.WriteLine("PostWorkloadEnvironmentRegion: Duplicate Environment-Region combination detected.");
                return BadRequest(new { error = "A Landing Zone with the same Environment and Region already exists for this Workload." });
            }

            _context.WorkloadEnvironmentRegions.Add(workloadEnvironmentRegion);
            await _context.SaveChangesAsync();

            Console.WriteLine("PostWorkloadEnvironmentRegion: Landing zone added successfully.");
            return CreatedAtAction(nameof(GetWorkloadEnvironmentRegion), new { id = workloadEnvironmentRegion.WorkloadEnvironmentRegionId }, workloadEnvironmentRegion);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PostWorkloadEnvironmentRegion: Error occurred - {ex.Message}");
            return StatusCode(500, "An error occurred while adding the landing zone.");
        }
    }

    // PUT: api/WorkloadEnvironmentRegions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWorkloadEnvironmentRegion(int id, WorkloadEnvironmentRegion workloadEnvironmentRegion)
    {
        if (id != workloadEnvironmentRegion.WorkloadEnvironmentRegionId)
        {
            return BadRequest();
        }

        _context.Entry(workloadEnvironmentRegion).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WorkloadEnvironmentRegionExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/WorkloadEnvironmentRegions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkloadEnvironmentRegion(int id)
    {
        var workloadEnvironmentRegion = await _context.WorkloadEnvironmentRegions.FindAsync(id);
        if (workloadEnvironmentRegion == null)
        {
            return NotFound();
        }

        _context.WorkloadEnvironmentRegions.Remove(workloadEnvironmentRegion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool WorkloadEnvironmentRegionExists(int id)
    {
        return _context.WorkloadEnvironmentRegions.Any(w => w.WorkloadEnvironmentRegionId == id);
    }
}
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

    // POST: api/WorkloadEnvironmentRegions
    [HttpPost]
    public async Task<ActionResult<WorkloadEnvironmentRegion>> PostWorkloadEnvironmentRegion(WorkloadEnvironmentRegion workloadEnvironmentRegion)
    {
        // Ignore navigation properties during validation
        ModelState.Remove(nameof(WorkloadEnvironmentRegion.Workload));
        ModelState.Remove(nameof(WorkloadEnvironmentRegion.EnvironmentType));
        ModelState.Remove(nameof(WorkloadEnvironmentRegion.Region));

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.WorkloadEnvironmentRegions.Add(workloadEnvironmentRegion);
        await _context.SaveChangesAsync();

        Console.WriteLine("Landing zone added successfully.");
        return CreatedAtAction(nameof(GetWorkloadEnvironmentRegion), new { id = workloadEnvironmentRegion.WorkloadEnvironmentRegionId }, workloadEnvironmentRegion);
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
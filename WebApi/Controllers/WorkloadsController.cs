// Controllers/WorkloadsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class WorkloadsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkloadsController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<Workload> IncludeRelatedEntities()
    {
        return _context.Workloads
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.EnvironmentType)
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.Region)
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.Resources)
                    .ThenInclude(r => r.ResourceType);
    }

    private WorkloadDto MapToDto(Workload workload)
    {
        return new WorkloadDto
        {
            WorkloadId = workload.WorkloadId,
            Name = workload.Name,
            Description = workload.Description,
            AzureNamePrefix = workload.AzureNamePrefix,
            PrimaryPOC = workload.PrimaryPOC,
            SecondaryPOC = workload.SecondaryPOC,
            LandingZonesCount = workload.WorkloadEnvironmentRegions.Count,
            ResourcesCount = workload.WorkloadEnvironmentRegions.Sum(wr => wr.Resources.Count),
            WorkloadEnvironmentRegions = workload.WorkloadEnvironmentRegions.Select(wr => new WorkloadEnvironmentRegionDto
            {
                WorkloadEnvironmentRegionId = wr.WorkloadEnvironmentRegionId,
                AzureSubscriptionId = wr.AzureSubscriptionId,
                ResourceGroupName = wr.ResourceGroupName,
                Name = wr.ResourceGroupName,
                EnvironmentTypeId = wr.EnvironmentTypeId,
                RegionId = wr.RegionId,
                EnvironmentTypeName = wr.EnvironmentType?.Name ?? string.Empty,
                RegionName = wr.Region?.Name ?? string.Empty,
                Resources = wr.Resources.Select(r => new ResourceDto
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
            }).ToList()
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkloadDto>>> GetWorkloads()
    {
        var workloads = await IncludeRelatedEntities().ToListAsync();
        var workloadDtos = workloads.Select(MapToDto);
        return Ok(workloadDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkloadDto>> GetWorkload(int id)
    {
        var workload = await IncludeRelatedEntities().FirstOrDefaultAsync(w => w.WorkloadId == id);

        if (workload == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(workload));
    }

    // POST: api/Workloads
    [HttpPost]
    public async Task<ActionResult<Workload>> PostWorkload(Workload workload)
    {
        _context.Workloads.Add(workload);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkload), new { id = workload.WorkloadId }, workload);
    }

    // PUT: api/Workloads/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWorkload(int id, Workload workload)
    {
        if (id != workload.WorkloadId)
        {
            return BadRequest();
        }

        _context.Entry(workload).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WorkloadExists(id))
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

    // DELETE: api/Workloads/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkload(int id)
    {
        var workload = await _context.Workloads.FindAsync(id);
        if (workload == null)
        {
            return NotFound();
        }

        _context.Workloads.Remove(workload);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool WorkloadExists(int id)
    {
        return _context.Workloads.Any(e => e.WorkloadId == id);
    }
}
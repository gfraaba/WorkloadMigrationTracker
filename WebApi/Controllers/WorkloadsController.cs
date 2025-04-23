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

    private Workload MapToModel(WorkloadDto workloadDto)
    {
        return new Workload
        {
            WorkloadId = workloadDto.WorkloadId,
            Name = workloadDto.Name,
            Description = workloadDto.Description,
            AzureNamePrefix = workloadDto.AzureNamePrefix,
            PrimaryPOC = workloadDto.PrimaryPOC,
            SecondaryPOC = workloadDto.SecondaryPOC
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
    public async Task<ActionResult<Workload>> PostWorkload(WorkloadDto workloadDto)
    {
        var workload = MapToModel(workloadDto);
        _context.Workloads.Add(workload);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkload), new { id = workload.WorkloadId }, workloadDto);
    }

    // PUT: api/Workloads/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWorkload(int id, WorkloadDto workloadDto)
    {
        if (id != workloadDto.WorkloadId)
        {
            return BadRequest();
        }

        var workload = await _context.Workloads.FindAsync(id);
        if (workload == null)
        {
            return NotFound();
        }

        // Update the properties of the tracked entity - MapToModel(workloadDto) would create a new instance, losing the tracking
        workload.Name = workloadDto.Name;
        workload.Description = workloadDto.Description;
        workload.AzureNamePrefix = workloadDto.AzureNamePrefix;
        workload.PrimaryPOC = workloadDto.PrimaryPOC;
        workload.SecondaryPOC = workloadDto.SecondaryPOC;

        try
        {
            // Save changes to the database
            await _context.SaveChangesAsync();
            Console.WriteLine($"WorkloadsController: Updated workload with ID {id}.");
        }
        catch (DbUpdateConcurrencyException)
        {
            Console.WriteLine($"WorkloadsController: Concurrency exception while updating workload with ID {id}.");
            throw;
        }

        return NoContent();
    }


    // DELETE: api/Workloads/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkload(int id)
    {
        var workload = await _context.Workloads
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.Resources)
                    .ThenInclude(r => r.PropertyValues) // Include PropertyValues
            .FirstOrDefaultAsync(w => w.WorkloadId == id);

        if (workload == null)
        {
            Console.WriteLine($"WorkloadsController: Workload with ID {id} not found.");
            return NotFound();
        }

        Console.WriteLine($"WorkloadsController: Deleting workload with ID {id}.");
        Console.WriteLine($"WorkloadsController: Found {workload.WorkloadEnvironmentRegions.Count} landing zones.");

        foreach (var region in workload.WorkloadEnvironmentRegions)
        {
            Console.WriteLine($"WorkloadsController: Landing Zone ID {region.WorkloadEnvironmentRegionId} has {region.Resources.Count} resources.");

            foreach (var resource in region.Resources)
            {
                Console.WriteLine($"WorkloadsController: Resource ID {resource.ResourceId} has {resource.PropertyValues.Count} property values.");
            }
        }

        _context.Workloads.Remove(workload);
        await _context.SaveChangesAsync();
        Console.WriteLine($"WorkloadsController: Deleted workload with ID {id}.");

        return NoContent();
    }
}
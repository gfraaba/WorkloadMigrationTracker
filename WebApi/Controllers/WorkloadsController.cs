// Controllers/WorkloadsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using Shared.Models;
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

    // GET: api/Workloads
    // [HttpGet]
    // public async Task<ActionResult<IEnumerable<Workload>>> GetWorkloads()
    // {
    //     return await _context.Workloads
    //         .Include(w => w.WorkloadEnvironmentRegions) // Ensure related data is included
    //         .ToListAsync();
    // }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkloadDto>>> GetWorkloads()
    {
        var workloads = await _context.Workloads
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.EnvironmentType)
            .Include(w => w.WorkloadEnvironmentRegions)
                .ThenInclude(wr => wr.Region)
            .Select(w => new WorkloadDto
            {
                WorkloadId = w.WorkloadId,
                Name = w.Name,
                Description = w.Description,
                AzureNamePrefix = w.AzureNamePrefix,
                PrimaryPOC = w.PrimaryPOC,
                SecondaryPOC = w.SecondaryPOC,
                LandingZonesCount = w.WorkloadEnvironmentRegions.Count,
                WorkloadEnvironmentRegions = w.WorkloadEnvironmentRegions.Select(wr => new WorkloadEnvironmentRegionDto
                {
                    WorkloadEnvironmentRegionId = wr.WorkloadEnvironmentRegionId,
                    AzureSubscriptionId = wr.AzureSubscriptionId,
                    ResourceGroupName = wr.ResourceGroupName,
                    EnvironmentTypeId = wr.EnvironmentTypeId,
                    RegionId = wr.RegionId,
                    EnvironmentTypeName = wr.EnvironmentType != null ? wr.EnvironmentType.Name : null,
                    RegionName = wr.Region != null ? wr.Region.Name : null
                }).ToList()
            })
            .ToListAsync();

        return Ok(workloads);
    }

    // GET: api/Workloads/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Workload>> GetWorkload(int id)
    {
        var workload = await _context.Workloads.FindAsync(id);

        if (workload == null)
        {
            return NotFound();
        }

        return workload;
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
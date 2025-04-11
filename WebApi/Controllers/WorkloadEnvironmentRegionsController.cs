using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using Shared.Models; // Updated namespace for Workload model

[ApiController]
[Route("api/[controller]")]
public class WorkloadEnvironmentRegionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkloadEnvironmentRegionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/WorkloadEnvironmentRegions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkloadEnvironmentRegion>>> GetWorkloadEnvironmentRegions()
    {
        return await _context.WorkloadEnvironmentRegions
            .Include(w => w.Workload)
            .Include(w => w.EnvironmentType)
            .Include(w => w.Region)
            .ToListAsync();
    }

    // GET: api/WorkloadEnvironmentRegions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkloadEnvironmentRegion>> GetWorkloadEnvironmentRegion(int id)
    {
        var workloadEnvironmentRegion = await _context.WorkloadEnvironmentRegions
            .Include(w => w.Workload)
            .Include(w => w.EnvironmentType)
            .Include(w => w.Region)
            .FirstOrDefaultAsync(w => w.WorkloadEnvironmentRegionId == id);

        if (workloadEnvironmentRegion == null)
        {
            return NotFound();
        }

        return workloadEnvironmentRegion;
    }

    // GET: api/WorkloadEnvironmentRegions/workload/{workloadId}
    [HttpGet("workload/{workloadId}")]
    public async Task<ActionResult<IEnumerable<WorkloadEnvironmentRegion>>> GetLandingZonesForWorkload(int workloadId)
    {
        var landingZones = await _context.WorkloadEnvironmentRegions
            .Include(w => w.EnvironmentType)
            .Include(w => w.Region)
            .Where(w => w.WorkloadId == workloadId)
            .ToListAsync();

        if (!landingZones.Any())
        {
            return NotFound($"No landing zones found for workload with ID {workloadId}.");
        }

        return Ok(landingZones);
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
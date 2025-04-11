// Controllers/ResourcesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourcesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Resources
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
    {
        return await _context.Resources
            .Include(r => r.Workload)
            .Include(r => r.WorkloadEnvironmentRegion)
            .Include(r => r.ResourceType)
            .Include(r => r.Status)
            .ToListAsync();
    }

    // GET: api/Resources/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetResource(int id)
    {
        var resource = await _context.Resources
            .Include(r => r.Workload)
            .Include(r => r.WorkloadEnvironmentRegion)
            .Include(r => r.ResourceType)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.ResourceId == id);

        if (resource == null)
        {
            return NotFound();
        }

        return resource;
    }

    // POST: api/Resources
    [HttpPost]
    public async Task<ActionResult<Resource>> PostResource(Resource resource)
    {
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResource), new { id = resource.ResourceId }, resource);
    }

    // PUT: api/Resources/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResource(int id, Resource resource)
    {
        if (id != resource.ResourceId)
        {
            return BadRequest();
        }

        _context.Entry(resource).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceExists(id))
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

    // DELETE: api/Resources/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("add-to-workload/{workloadId}")]
    public async Task<IActionResult> AddResourceToWorkload(int workloadId, [FromBody] Resource resource)
    {
        Console.WriteLine($"ResourcesController: Received request to add resource to workload {workloadId}.");
        Console.WriteLine($"Resource Details: Name={resource.Name}, TypeId={resource.TypeId}, StatusId={resource.StatusId}");

        // Ensure WorkloadEnvironmentRegion exists
        var workloadEnvironmentRegion = await _context.WorkloadEnvironmentRegions
            .FirstOrDefaultAsync(w => w.WorkloadId == workloadId &&
                                     w.EnvironmentTypeId == resource.WorkloadEnvironmentRegion.EnvironmentType.EnvironmentTypeId &&
                                     w.RegionId == resource.WorkloadEnvironmentRegion.Region.RegionId);

        if (workloadEnvironmentRegion == null)
        {
            workloadEnvironmentRegion = new WorkloadEnvironmentRegion
            {
                WorkloadId = workloadId,
                EnvironmentTypeId = resource.WorkloadEnvironmentRegion.EnvironmentType.EnvironmentTypeId,
                RegionId = resource.WorkloadEnvironmentRegion.Region.RegionId,
                Workload = await _context.Workloads.FindAsync(workloadId), // Use existing Workload
                EnvironmentType = await _context.EnvironmentTypes.FindAsync(resource.WorkloadEnvironmentRegion.EnvironmentType.EnvironmentTypeId), // Use existing EnvironmentType
                Region = await _context.AzureRegions.FindAsync(resource.WorkloadEnvironmentRegion.Region.RegionId) // Use existing Region
            };
            _context.WorkloadEnvironmentRegions.Add(workloadEnvironmentRegion);
            await _context.SaveChangesAsync();
        }

        resource.WorkloadEnvironmentRegionId = workloadEnvironmentRegion.WorkloadEnvironmentRegionId;

        try
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            Console.WriteLine("ResourcesController: Resource added successfully.");
            return Ok(resource);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ResourcesController: Error adding resource - {ex.Message}");
            return StatusCode(500, "An error occurred while adding the resource.");
        }
    }

    private bool ResourceExists(int id)
    {
        return _context.Resources.Any(e => e.ResourceId == id);
    }
}
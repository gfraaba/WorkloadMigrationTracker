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
            .Include(r => r.WorkloadEnvironmentRegion)
            .Include(r => r.ResourceType)
            .ToListAsync();
    }

    // GET: api/Resources/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetResource(int id)
    {
        var resource = await _context.Resources
            .Include(r => r.WorkloadEnvironmentRegion)
            .Include(r => r.ResourceType)
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

    [HttpPost("add-to-workload/{workloadEnvironmentRegionId}")]
    public async Task<IActionResult> AddResourceToWorkload(int workloadEnvironmentRegionId, [FromBody] Resource resource)
    {
        Console.WriteLine($"ResourcesController: Received request to add resource to LZ Id {workloadEnvironmentRegionId}.");
        Console.WriteLine($"Resource Details: Name={resource.Name}, ResourceTypeId={resource.ResourceTypeId}, Status={resource.Status}");

        // Ensure WorkloadEnvironmentRegion exists
        var workloadEnvironmentRegion = await _context.WorkloadEnvironmentRegions
            .FirstOrDefaultAsync(w => w.WorkloadEnvironmentRegionId == workloadEnvironmentRegionId);
        if (workloadEnvironmentRegion == null)
        {
            Console.WriteLine($"ResourcesController: WorkloadEnvironmentRegion with ID {workloadEnvironmentRegionId} not found.");
            return NotFound(new { error = $"WorkloadEnvironmentRegion with ID {workloadEnvironmentRegionId} not found." });
        }

        var resourceTypeExists = await _context.ResourceTypes.AnyAsync(rt => rt.TypeId == resource.ResourceTypeId);
        if (!resourceTypeExists)
        {
            Console.WriteLine($"ResourcesController: ResourceType with ID {resource.ResourceTypeId} not found.");
            return BadRequest(new { error = $"ResourceType with ID {resource.ResourceTypeId} not found." });
        }

        var validStatuses = new[] { "Available", "Unavailable", "InProgress" }; // Example statuses
        if (!validStatuses.Contains(resource.Status))
        {
            Console.WriteLine($"ResourcesController: Invalid status '{resource.Status}'.");
            return BadRequest(new { error = $"Invalid status '{resource.Status}'. Valid statuses are: {string.Join(", ", validStatuses)}." });
        }

        try
        {
            resource.WorkloadEnvironmentRegionId = workloadEnvironmentRegionId; // Ensure the foreign key is set
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
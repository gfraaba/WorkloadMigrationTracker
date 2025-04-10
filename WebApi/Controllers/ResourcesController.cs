// Controllers/ResourcesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

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

    private bool ResourceExists(int id)
    {
        return _context.Resources.Any(e => e.ResourceId == id);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ResourceTypesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceTypesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ResourceTypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceType>>> GetResourceTypes()
    {
        return await _context.ResourceTypes
            .Include(rt => rt.Category) // Include related ResourceCategory
            .ToListAsync();
    }

    // GET: api/ResourceTypes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceType>> GetResourceType(int id)
    {
        var resourceType = await _context.ResourceTypes
            .Include(rt => rt.Category) // Include related ResourceCategory
            .FirstOrDefaultAsync(rt => rt.TypeId == id);

        if (resourceType == null)
        {
            return NotFound();
        }

        return resourceType;
    }

    // POST: api/ResourceTypes
    [HttpPost]
    public async Task<ActionResult<ResourceType>> PostResourceType(ResourceType resourceType)
    {
        _context.ResourceTypes.Add(resourceType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResourceType), new { id = resourceType.TypeId }, resourceType);
    }

    // PUT: api/ResourceTypes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResourceType(int id, ResourceType resourceType)
    {
        if (id != resourceType.TypeId)
        {
            return BadRequest();
        }

        _context.Entry(resourceType).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceTypeExists(id))
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

    // DELETE: api/ResourceTypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResourceType(int id)
    {
        var resourceType = await _context.ResourceTypes.FindAsync(id);
        if (resourceType == null)
        {
            return NotFound();
        }

        _context.ResourceTypes.Remove(resourceType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ResourceTypeExists(int id)
    {
        return _context.ResourceTypes.Any(rt => rt.TypeId == id);
    }
}
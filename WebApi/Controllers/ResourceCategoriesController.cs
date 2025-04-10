using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ResourceCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ResourceCategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceCategory>>> GetResourceCategories()
    {
        return await _context.ResourceCategories
            .Include(rc => rc.ResourceTypes) // Include related ResourceTypes
            .ToListAsync();
    }

    // GET: api/ResourceCategories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceCategory>> GetResourceCategory(int id)
    {
        var resourceCategory = await _context.ResourceCategories
            .Include(rc => rc.ResourceTypes) // Include related ResourceTypes
            .FirstOrDefaultAsync(rc => rc.CategoryId == id);

        if (resourceCategory == null)
        {
            return NotFound();
        }

        return resourceCategory;
    }

    // POST: api/ResourceCategories
    [HttpPost]
    public async Task<ActionResult<ResourceCategory>> PostResourceCategory(ResourceCategory resourceCategory)
    {
        _context.ResourceCategories.Add(resourceCategory);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResourceCategory), new { id = resourceCategory.CategoryId }, resourceCategory);
    }

    // PUT: api/ResourceCategories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResourceCategory(int id, ResourceCategory resourceCategory)
    {
        if (id != resourceCategory.CategoryId)
        {
            return BadRequest();
        }

        _context.Entry(resourceCategory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceCategoryExists(id))
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

    // DELETE: api/ResourceCategories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResourceCategory(int id)
    {
        var resourceCategory = await _context.ResourceCategories.FindAsync(id);
        if (resourceCategory == null)
        {
            return NotFound();
        }

        _context.ResourceCategories.Remove(resourceCategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ResourceCategoryExists(int id)
    {
        return _context.ResourceCategories.Any(rc => rc.CategoryId == id);
    }
}
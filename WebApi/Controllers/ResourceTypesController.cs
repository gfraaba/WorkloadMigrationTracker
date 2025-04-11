using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ResourceTypesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceTypesController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<ResourceType> IncludeRelatedEntities()
    {
        return _context.ResourceTypes.Include(rt => rt.Category);
    }

    private ResourceTypeDto MapToDto(ResourceType resourceType)
    {
        return new ResourceTypeDto
        {
            TypeId = resourceType.TypeId,
            Name = resourceType.Name,
            AzureResourceType = resourceType.AzureResourceType,
            CategoryId = resourceType.CategoryId,
            Category = resourceType.Category != null ? new ResourceCategoryDto
            {
                CategoryId = resourceType.Category.CategoryId,
                Name = resourceType.Category.Name,
                AzureServiceType = resourceType.Category.AzureServiceType
            } : null
        };
    }

    // GET: api/ResourceTypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceTypeDto>>> GetResourceTypes()
    {
        var resourceTypes = await IncludeRelatedEntities().ToListAsync();
        var resourceTypeDtos = resourceTypes.Select(MapToDto);
        return Ok(resourceTypeDtos);
    }

    // GET: api/ResourceTypes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceTypeDto>> GetResourceType(int id)
    {
        var resourceType = await IncludeRelatedEntities().FirstOrDefaultAsync(rt => rt.TypeId == id);

        if (resourceType == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resourceType));
    }

    // POST: api/ResourceTypes
    [HttpPost]
    public async Task<ActionResult<ResourceType>> PostResourceType(ResourceType resourceType)
    {
        // Ensure the CategoryId is valid
        if (!await IsCategoryIdValid(resourceType.CategoryId))
        {
            return BadRequest(new { error = "Invalid CategoryId!" });
        }
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
            return BadRequest(new { error = "TypeId doesn't match!" });
        }

        // Ensure the CategoryId is valid
        if (!await IsCategoryIdValid(resourceType.CategoryId))
        {
            return BadRequest(new { error = "Invalid CategoryId!" });
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

    private async Task<bool> IsCategoryIdValid(int categoryId)
    {
        return await _context.ResourceCategories.AnyAsync(c => c.CategoryId == categoryId);
    }
}
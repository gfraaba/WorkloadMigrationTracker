using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ResourceCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<ResourceCategory> IncludeRelatedEntities()
    {
        return _context.ResourceCategories.Include(rc => rc.ResourceTypes);
    }

    private ResourceCategoryDto MapToDto(ResourceCategory resourceCategory)
    {
        return new ResourceCategoryDto
        {
            CategoryId = resourceCategory.CategoryId,
            Name = resourceCategory.Name,
            AzureServiceType = resourceCategory.AzureServiceType,
            ResourceTypes = resourceCategory.ResourceTypes.Select(rt => new ResourceTypeDto
            {
                TypeId = rt.TypeId,
                Name = rt.Name,
                AzureResourceType = rt.AzureResourceType,
                CategoryId = rt.CategoryId
            }).ToList()
        };
    }

    // GET: api/ResourceCategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceCategoryDto>>> GetResourceCategories()
    {
        var resourceCategories = await IncludeRelatedEntities().ToListAsync();
        var resourceCategoryDtos = resourceCategories.Select(MapToDto);
        return Ok(resourceCategoryDtos);
    }

    // GET: api/ResourceCategories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceCategoryDto>> GetResourceCategory(int id)
    {
        var resourceCategory = await IncludeRelatedEntities().FirstOrDefaultAsync(rc => rc.CategoryId == id);

        if (resourceCategory == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resourceCategory));
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
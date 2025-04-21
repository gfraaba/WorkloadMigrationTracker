using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ResourcePropertiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourcePropertiesController(AppDbContext context)
    {
        _context = context;
    }

    private ResourcePropertyDto MapToDto(ResourceProperty resourceProperty)
    {
        return new ResourcePropertyDto
        {
            PropertyId = resourceProperty.PropertyId,
            ResourceTypeId = resourceProperty.ResourceTypeId,
            Name = resourceProperty.Name,
            DataType = resourceProperty.DataType,
            IsRequired = resourceProperty.IsRequired,
            DefaultValue = resourceProperty.DefaultValue
        };
    }

    [HttpGet("{resourceTypeId}")]
    public async Task<ActionResult<IEnumerable<ResourcePropertyDto>>> GetResourceProperties(int resourceTypeId)
    {
        var properties = await _context.ResourceProperties
            .Where(rp => rp.ResourceTypeId == resourceTypeId)
            .ToListAsync();

        if (!properties.Any())
        {
            return NotFound();
        }

        var propertyDtos = properties.Select(MapToDto);
        return Ok(propertyDtos);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceProperty>> PostResourceProperty(ResourceProperty resourceProperty)
    {
        // Ensure the ResourceTypeId is valid
        if (!_context.ResourceTypes.Any(rt => rt.TypeId == resourceProperty.ResourceTypeId))
        {
            return BadRequest(new { error = "Invalid ResourceTypeId!" });
        }

        _context.ResourceProperties.Add(resourceProperty);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResourceProperties), new { resourceTypeId = resourceProperty.ResourceTypeId }, resourceProperty);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutResourceProperty(int id, ResourceProperty resourceProperty)
    {
        if (id != resourceProperty.PropertyId)
        {
            return BadRequest(new { error = "PropertyId doesn't match!" });
        }

        _context.Entry(resourceProperty).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.ResourceProperties.Any(rp => rp.PropertyId == id))
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResourceProperty(int id)
    {
        var resourceProperty = await _context.ResourceProperties.FindAsync(id);
        if (resourceProperty == null)
        {
            return NotFound();
        }

        _context.ResourceProperties.Remove(resourceProperty);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
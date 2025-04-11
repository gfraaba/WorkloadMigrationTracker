using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ResourceStatusesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceStatusesController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<ResourceStatus> IncludeRelatedEntities()
    {
        return _context.ResourceStatuses;
    }

    private ResourceStatusDto MapToDto(ResourceStatus resourceStatus)
    {
        return new ResourceStatusDto
        {
            StatusId = resourceStatus.StatusId,
            Name = resourceStatus.Name
        };
    }

    // GET: api/ResourceStatuses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceStatusDto>>> GetResourceStatuses()
    {
        var resourceStatuses = await IncludeRelatedEntities().ToListAsync();

        if (!resourceStatuses.Any())
        {
            return Ok(new List<ResourceStatusDto>()); // Return an empty list with a 200 status code
        }

        var resourceStatusDtos = resourceStatuses.Select(MapToDto);
        return Ok(resourceStatusDtos);
    }

    // GET: api/ResourceStatuses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceStatusDto>> GetResourceStatus(int id)
    {
        var resourceStatus = await IncludeRelatedEntities().FirstOrDefaultAsync(rs => rs.StatusId == id);

        if (resourceStatus == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resourceStatus));
    }

    // POST: api/ResourceStatuses
    [HttpPost]
    public async Task<ActionResult<ResourceStatus>> PostResourceStatus(ResourceStatus resourceStatus)
    {
        _context.ResourceStatuses.Add(resourceStatus);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResourceStatus), new { id = resourceStatus.StatusId }, resourceStatus);
    }

    // PUT: api/ResourceStatuses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResourceStatus(int id, ResourceStatus resourceStatus)
    {
        if (id != resourceStatus.StatusId)
        {
            return BadRequest();
        }

        _context.Entry(resourceStatus).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceStatusExists(id))
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

    // DELETE: api/ResourceStatuses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResourceStatus(int id)
    {
        var resourceStatus = await _context.ResourceStatuses.FindAsync(id);
        if (resourceStatus == null)
        {
            return NotFound();
        }

        _context.ResourceStatuses.Remove(resourceStatus);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ResourceStatusExists(int id)
    {
        return _context.ResourceStatuses.Any(rs => rs.StatusId == id);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AzureRegionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AzureRegionsController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<AzureRegion> IncludeRelatedEntities()
    {
        return _context.AzureRegions;
    }

    private AzureRegionDto MapToDto(AzureRegion azureRegion)
    {
        return new AzureRegionDto
        {
            RegionId = azureRegion.RegionId,
            Code = azureRegion.Code,
            Name = azureRegion.Name,
            IsActive = azureRegion.IsActive
        };
    }

    // GET: api/AzureRegions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AzureRegionDto>>> GetAzureRegions()
    {
        var azureRegions = await IncludeRelatedEntities().ToListAsync();

        if (!azureRegions.Any())
        {
            return Ok(new List<AzureRegionDto>()); // Return an empty list with a 200 status code
        }

        var azureRegionDtos = azureRegions.Select(MapToDto);
        return Ok(azureRegionDtos);
    }

    // GET: api/AzureRegions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AzureRegionDto>> GetAzureRegion(int id)
    {
        var azureRegion = await IncludeRelatedEntities().FirstOrDefaultAsync(ar => ar.RegionId == id);

        if (azureRegion == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(azureRegion));
    }

    // GET: api/AzureRegions/regions
    [HttpGet("regions")]
    public async Task<ActionResult<IEnumerable<AzureRegion>>> GetRegions()
    {
        return await _context.AzureRegions.ToListAsync();
    }

    // POST: api/AzureRegions
    [HttpPost]
    public async Task<ActionResult<AzureRegion>> PostAzureRegion(AzureRegion azureRegion)
    {
        _context.AzureRegions.Add(azureRegion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAzureRegion), new { id = azureRegion.RegionId }, azureRegion);
    }

    // PUT: api/AzureRegions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAzureRegion(int id, AzureRegion azureRegion)
    {
        if (id != azureRegion.RegionId)
        {
            return BadRequest();
        }

        _context.Entry(azureRegion).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AzureRegionExists(id))
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

    // DELETE: api/AzureRegions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAzureRegion(int id)
    {
        var azureRegion = await _context.AzureRegions.FindAsync(id);
        if (azureRegion == null)
        {
            return NotFound();
        }

        _context.AzureRegions.Remove(azureRegion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AzureRegionExists(int id)
    {
        return _context.AzureRegions.Any(ar => ar.RegionId == id);
    }
}
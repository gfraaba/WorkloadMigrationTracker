using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class AzureRegionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AzureRegionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/AzureRegions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AzureRegion>>> GetAzureRegions()
    {
        return await _context.AzureRegions.ToListAsync();
    }

    // GET: api/AzureRegions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AzureRegion>> GetAzureRegion(int id)
    {
        var azureRegion = await _context.AzureRegions.FindAsync(id);

        if (azureRegion == null)
        {
            return NotFound();
        }

        return azureRegion;
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
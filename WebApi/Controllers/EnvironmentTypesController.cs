using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentTypesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnvironmentTypesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/EnvironmentTypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnvironmentType>>> GetEnvironmentTypes()
    {
        return await _context.EnvironmentTypes.ToListAsync();
    }

    // GET: api/EnvironmentTypes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EnvironmentType>> GetEnvironmentType(int id)
    {
        var environmentType = await _context.EnvironmentTypes.FindAsync(id);

        if (environmentType == null)
        {
            return NotFound();
        }

        return environmentType;
    }

    // POST: api/EnvironmentTypes
    [HttpPost]
    public async Task<ActionResult<EnvironmentType>> PostEnvironmentType(EnvironmentType environmentType)
    {
        _context.EnvironmentTypes.Add(environmentType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEnvironmentType), new { id = environmentType.EnvironmentTypeId }, environmentType);
    }

    // PUT: api/EnvironmentTypes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEnvironmentType(int id, EnvironmentType environmentType)
    {
        if (id != environmentType.EnvironmentTypeId)
        {
            return BadRequest();
        }

        _context.Entry(environmentType).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EnvironmentTypeExists(id))
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

    // DELETE: api/EnvironmentTypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnvironmentType(int id)
    {
        var environmentType = await _context.EnvironmentTypes.FindAsync(id);
        if (environmentType == null)
        {
            return NotFound();
        }

        _context.EnvironmentTypes.Remove(environmentType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EnvironmentTypeExists(int id)
    {
        return _context.EnvironmentTypes.Any(et => et.EnvironmentTypeId == id);
    }
}
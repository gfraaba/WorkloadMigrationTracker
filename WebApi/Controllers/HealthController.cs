using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebApi.Data;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    // Basic health check
    [HttpGet]
    public IActionResult CheckHealth()
    {
        return Ok(new 
        { 
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }

    // Database connectivity health check
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabaseHealth()
    {
        try
        {
            // Simple query to test database connection
            bool canConnectToDatabase = await _context.Database.CanConnectAsync();
            
            if (canConnectToDatabase)
            {
                return Ok(new
                {
                    status = "Healthy",
                    database = "Connected",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    status = "Unhealthy",
                    database = "Failed to connect",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "Unhealthy",
                database = "Error",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
// Controllers/ResourcesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourcesController(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<Resource> IncludeRelatedEntities()
    {
        return _context.Resources
            .Include(r => r.WorkloadEnvironmentRegion)
            .Include(r => r.ResourceType)
            .ThenInclude(r => r.Category)
            .Include(r => r.PropertyValues);
    }

    private ResourceDto MapToDto(Resource resource)
    {
        return new ResourceDto
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            WorkloadEnvironmentRegionId = resource.WorkloadEnvironmentRegionId,
            ResourceTypeId = resource.ResourceTypeId,
            Status = resource.Status,
            ResourceType = resource.ResourceType != null ? new ResourceTypeDto
            {
                TypeId = resource.ResourceType.TypeId,
                Name = resource.ResourceType.Name,
                AzureResourceType = resource.ResourceType.AzureResourceType,
                CategoryId = resource.ResourceType.CategoryId,
                Category = resource.ResourceType.Category != null ? new ResourceCategoryDto
                {
                    CategoryId = resource.ResourceType.Category.CategoryId,
                    Name = resource.ResourceType.Category.Name
                } : null
                // ,Properties = resource.ResourceType.Properties.Select(rp => new ResourcePropertyDto
                // {
                //     PropertyId = rp.PropertyId,
                //     ResourceTypeId = rp.ResourceTypeId,
                //     Name = rp.Name,
                //     DataType = rp.DataType,
                //     IsRequired = rp.IsRequired,
                //     DefaultValue = rp.DefaultValue
                // }).ToList()
            } : null,
            WorkloadEnvironmentRegion = resource.WorkloadEnvironmentRegion != null ? new WorkloadEnvironmentRegionDto
            {
                WorkloadEnvironmentRegionId = resource.WorkloadEnvironmentRegion.WorkloadEnvironmentRegionId,
                AzureSubscriptionId = resource.WorkloadEnvironmentRegion.AzureSubscriptionId,
                ResourceGroupName = resource.WorkloadEnvironmentRegion.ResourceGroupName,
                EnvironmentTypeId = resource.WorkloadEnvironmentRegion.EnvironmentTypeId,
                RegionId = resource.WorkloadEnvironmentRegion.RegionId
            } : null,
            Properties = resource.PropertyValues.ToDictionary(
                pv => _context.ResourceProperties.FirstOrDefault(rp => rp.PropertyId == pv.PropertyId)?.Name ?? string.Empty,
                pv => (object)pv.Value
            )
        };
    }

    // GET: api/Resources
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources()
    {
        var resources = await IncludeRelatedEntities().ToListAsync();
        var resourceDtos = resources.Select(MapToDto);
        return Ok(resourceDtos);
    }

    // GET: api/Resources/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetResource(int id)
    {
        var resource = await IncludeRelatedEntities().FirstOrDefaultAsync(r => r.ResourceId == id);

        if (resource == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resource));
    }

    // GET: api/Resources/landing-zone/{landingZoneId}
    [HttpGet("landing-zone/{landingZoneId}")]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResourcesForLandingZone(int landingZoneId)
    {
        var resources = await IncludeRelatedEntities()
            .Where(r => r.WorkloadEnvironmentRegionId == landingZoneId)
            .ToListAsync();

        if (!resources.Any())
        {
            return Ok(new List<ResourceDto>()); // Return an empty list with a 200 status code
        }

        var resourceDtos = resources.Select(MapToDto);
        return Ok(resourceDtos);
    }

    // // POST: api/Resources
    // [HttpPost]
    // public async Task<ActionResult<Resource>> PostResource(Resource resource)
    // {
    //     _context.Resources.Add(resource);
    //     await _context.SaveChangesAsync();

    //     return CreatedAtAction(nameof(GetResource), new { id = resource.ResourceId }, resource);
    // }

    // PUT: api/Resources/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResource(int id, Resource resource)
    {
        if (id != resource.ResourceId)
        {
            return BadRequest();
        }

        _context.Entry(resource).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceExists(id))
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

    // DELETE: api/Resources/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> AddResource([FromBody] ResourceDto resourceDto)
    {
        Console.WriteLine($"ResourcesController: Received request to add resource to LZ Id {resourceDto.WorkloadEnvironmentRegionId}.");
        Console.WriteLine($"Resource Details: Name={resourceDto.Name}, ResourceTypeId={resourceDto.ResourceTypeId}, Status={resourceDto.Status}");

        // Ensure WorkloadEnvironmentRegion exists
        var workloadEnvironmentRegion = await _context.WorkloadEnvironmentRegions
            .FirstOrDefaultAsync(w => w.WorkloadEnvironmentRegionId == resourceDto.WorkloadEnvironmentRegionId);
        if (workloadEnvironmentRegion == null)
        {
            Console.WriteLine($"ResourcesController: WorkloadEnvironmentRegion with ID {resourceDto.WorkloadEnvironmentRegionId} not found.");
            return NotFound(new { error = $"WorkloadEnvironmentRegion with ID {resourceDto.WorkloadEnvironmentRegionId} not found." });
        }

        // Ensure ResourceType exists
        var resourceType = await _context.ResourceTypes
            .Include(rt => rt.Properties)
            .FirstOrDefaultAsync(rt => rt.TypeId == resourceDto.ResourceTypeId);
        if (resourceType == null)
        {
            Console.WriteLine($"ResourcesController: ResourceType with ID {resourceDto.ResourceTypeId} not found.");
            return BadRequest(new { error = $"ResourceType with ID {resourceDto.ResourceTypeId} not found." });
        }

        // Validate required properties
        foreach (var property in resourceType.Properties.Where(p => p.IsRequired))
        {
            if (!resourceDto.Properties.ContainsKey(property.Name) || resourceDto.Properties[property.Name] == null)
            {
                return BadRequest(new { error = $"Missing required property: {property.Name}" });
            }
        }

        // Add the base Resource
        var resource = new Resource
        {
            Name = resourceDto.Name,
            WorkloadEnvironmentRegionId = resourceDto.WorkloadEnvironmentRegionId,
            ResourceTypeId = resourceDto.ResourceTypeId,
            Status = resourceDto.Status
        };

        try
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            Console.WriteLine("ResourcesController: Resource added successfully.");

            // Add ResourcePropertyValues
            foreach (var property in resourceDto.Properties)
            {
                var propertyMetadata = resourceType.Properties.FirstOrDefault(p => p.Name == property.Key);
                if (propertyMetadata != null)
                {
                    var propertyValue = new ResourcePropertyValue
                    {
                        ResourceId = resource.ResourceId,
                        PropertyId = propertyMetadata.PropertyId,
                        Value = property.Value.ToString() ?? string.Empty
                    };
                    _context.ResourcePropertyValues.Add(propertyValue);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(resourceDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ResourcesController: Error adding resource - {ex.Message}");
            return StatusCode(500, "An error occurred while adding the resource.");
        }
    }

    private bool ResourceExists(int id)
    {
        return _context.Resources.Any(e => e.ResourceId == id);
    }
}
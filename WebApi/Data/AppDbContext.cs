using Microsoft.EntityFrameworkCore;
using Shared.Models; // Updated namespace for Workload model

namespace WebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workload> Workloads { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<EnvironmentType> EnvironmentTypes { get; set; }
    public DbSet<AzureRegion> AzureRegions { get; set; }
    public DbSet<ResourceCategory> ResourceCategories { get; set; }
    public DbSet<ResourceType> ResourceTypes { get; set; }
    public DbSet<WorkloadEnvironmentRegion> WorkloadEnvironmentRegions { get; set; }
    public DbSet<ResourceStatus> ResourceStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly configure AzureRegion's primary key
        modelBuilder.Entity<AzureRegion>(entity =>
        {
            entity.HasKey(e => e.RegionId); // ← This is critical
            entity.Property(e => e.RegionId).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Configure ResourceCategory FIRST
        modelBuilder.Entity<ResourceCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId); // Explicit primary key
            entity.HasIndex(e => e.Name).IsUnique(); // Unique constraint
            // entity.Navigation(e => e.ResourceTypes).AutoInclude(false); // Prevent automatic inclusion of ResourceTypes
        });

        // Configure ALL entities with their primary keys first
        modelBuilder.Entity<ResourceStatus>(entity => 
        {
            entity.HasKey(e => e.StatusId);
        });

        modelBuilder.Entity<ResourceType>(entity =>
        {
            // 1. Primary Key (MUST come first)
            entity.HasKey(rt => rt.TypeId);

            // 2. Unique Composite Index (as per your SQL schema)
            entity.HasIndex(rt => new { rt.CategoryId, rt.Name })
                .IsUnique();

            // 3. Relationship Configuration
            entity.HasOne(rt => rt.Category)
                .WithMany(c => c.ResourceTypes)
                .HasForeignKey(rt => rt.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Or your preferred delete behavior
        });

        // Configure primary keys and relationships
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasOne(r => r.WorkloadEnvironmentRegion)
                  .WithMany(r => r.Resources)
                  .HasForeignKey(r => r.WorkloadEnvironmentRegionId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            entity.HasOne(r => r.ResourceType)
                  .WithMany(s => s.Resources)
                  .HasForeignKey(r => r.ResourceTypeId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            entity.HasOne(r => r.Status)
                  .WithMany(s => s.Resources)
                  .HasForeignKey(r => r.StatusId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            entity.HasOne(r => r.Workload)
                .WithMany(w => w.Resources)
                .HasForeignKey(r => r.WorkloadId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        });

        modelBuilder.Entity<WorkloadEnvironmentRegion>(entity =>
        {
            entity.HasIndex(w => new { w.WorkloadId, w.EnvironmentTypeId, w.RegionId }).IsUnique();
        });

        // Seed data - EXACTLY matches your SQL script
        SeedData(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Save changes to the database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Automatically load navigation properties for newly added entities
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                // Load navigation properties for the added entity
                foreach (var navigation in entry.Navigations)
                {
                    if (!navigation.IsLoaded)
                    {
                        await navigation.LoadAsync(cancellationToken);
                    }
                }
            }
        }

        return result;
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Environment Types
        modelBuilder.Entity<EnvironmentType>().HasData(
            new EnvironmentType { EnvironmentTypeId = 1, Name = "Dev", Description = "Development Environment" },
            new EnvironmentType { EnvironmentTypeId = 2, Name = "QA", Description = "Quality Assurance Environment" },
            new EnvironmentType { EnvironmentTypeId = 3, Name = "Prod", Description = "Production Environment" }
        );

        // Azure Regions
        modelBuilder.Entity<AzureRegion>().HasData(
            new AzureRegion { RegionId = 1, Code = "eastus", Name = "East US", IsActive = true },
            new AzureRegion { RegionId = 2, Code = "westus", Name = "West US", IsActive = true }
        );

        // Resource Categories
        modelBuilder.Entity<ResourceCategory>().HasData(
            new ResourceCategory { CategoryId = 1, Name = "Compute", AzureServiceType = "Microsoft.Compute" },
            new ResourceCategory { CategoryId = 2, Name = "Storage", AzureServiceType = "Microsoft.Storage" },
            new ResourceCategory { CategoryId = 3, Name = "Network", AzureServiceType = "Microsoft.Network" },
            new ResourceCategory { CategoryId = 4, Name = "Database", AzureServiceType = "Microsoft.Database" },
            new ResourceCategory { CategoryId = 5, Name = "Monitoring", AzureServiceType = "Microsoft.Insights" },
            new ResourceCategory { CategoryId = 6, Name = "Security", AzureServiceType = "Microsoft.Security" }
        );

        // Resource Statuses
        modelBuilder.Entity<ResourceStatus>().HasData(
            new ResourceStatus { StatusId = 1, Name = "Unavailable" },
            new ResourceStatus { StatusId = 2, Name = "Deployed" },
            new ResourceStatus { StatusId = 3, Name = "Ready" }
        );

        // Resource Types
        modelBuilder.Entity<ResourceType>().HasData(
            // Compute
            new ResourceType { TypeId = 1, CategoryId = 1, Name = "Virtual Machine", AzureResourceType = "Microsoft.Compute/virtualMachines" },
            new ResourceType { TypeId = 2, CategoryId = 1, Name = "VM Scale Set", AzureResourceType = "Microsoft.Compute/virtualMachineScaleSets" },
            new ResourceType { TypeId = 3, CategoryId = 1, Name = "App Service", AzureResourceType = "Microsoft.Web/sites" },
            new ResourceType { TypeId = 4, CategoryId = 1, Name = "Function App", AzureResourceType = "Microsoft.Web/sites/functions" },
            new ResourceType { TypeId = 5, CategoryId = 1, Name = "Container Instance", AzureResourceType = "Microsoft.ContainerInstance/containerGroups" },
            new ResourceType { TypeId = 6, CategoryId = 1, Name = "Kubernetes Service", AzureResourceType = "Microsoft.ContainerService/managedClusters" },
            
            // Storage
            new ResourceType { TypeId = 7, CategoryId = 2, Name = "Storage Account", AzureResourceType = "Microsoft.Storage/storageAccounts" },
            new ResourceType { TypeId = 8, CategoryId = 2, Name = "Blob Container", AzureResourceType = "Microsoft.Storage/storageAccounts/blobServices/containers" },
            new ResourceType { TypeId = 9, CategoryId = 2, Name = "File Share", AzureResourceType = "Microsoft.Storage/storageAccounts/fileServices/shares" },
            new ResourceType { TypeId = 10, CategoryId = 2, Name = "Queue", AzureResourceType = "Microsoft.Storage/storageAccounts/queueServices/queues" },
            new ResourceType { TypeId = 11, CategoryId = 2, Name = "Table", AzureResourceType = "Microsoft.Storage/storageAccounts/tableServices/tables" },
            new ResourceType { TypeId = 12, CategoryId = 2, Name = "Disk", AzureResourceType = "Microsoft.Compute/disks" },
            
            // Networking
            new ResourceType { TypeId = 13, CategoryId = 3, Name = "Virtual Network", AzureResourceType = "Microsoft.Network/virtualNetworks" },
            new ResourceType { TypeId = 14, CategoryId = 3, Name = "Subnet", AzureResourceType = "Microsoft.Network/virtualNetworks/subnets" },
            new ResourceType { TypeId = 15, CategoryId = 3, Name = "Network Security Group", AzureResourceType = "Microsoft.Network/networkSecurityGroups" },
            new ResourceType { TypeId = 16, CategoryId = 3, Name = "Load Balancer", AzureResourceType = "Microsoft.Network/loadBalancers" },
            new ResourceType { TypeId = 17, CategoryId = 3, Name = "Application Gateway", AzureResourceType = "Microsoft.Network/applicationGateways" },
            new ResourceType { TypeId = 18, CategoryId = 3, Name = "Front Door", AzureResourceType = "Microsoft.Network/frontDoors" },
            
            // Databases
            new ResourceType { TypeId = 19, CategoryId = 4, Name = "SQL Database", AzureResourceType = "Microsoft.Sql/servers/databases" },
            new ResourceType { TypeId = 20, CategoryId = 4, Name = "Cosmos DB", AzureResourceType = "Microsoft.DocumentDB/databaseAccounts" },
            new ResourceType { TypeId = 21, CategoryId = 4, Name = "Redis Cache", AzureResourceType = "Microsoft.Cache/Redis" },
            
            // Monitoring
            new ResourceType { TypeId = 22, CategoryId = 5, Name = "Log Analytics Workspace", AzureResourceType = "Microsoft.OperationalInsights/workspaces" },
            new ResourceType { TypeId = 23, CategoryId = 5, Name = "Application Insights", AzureResourceType = "Microsoft.Insights/components" },
            
            // Security
            new ResourceType { TypeId = 24, CategoryId = 6, Name = "Key Vault", AzureResourceType = "Microsoft.KeyVault/vaults" },
            new ResourceType { TypeId = 25, CategoryId = 6, Name = "Managed Identity", AzureResourceType = "Microsoft.ManagedIdentity/userAssignedIdentities" }
        );
    }
}
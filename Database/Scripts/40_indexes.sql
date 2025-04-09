USE WorkloadMigration;
GO

BEGIN TRANSACTION;
    -- Execute all index creations here

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkloadEnvironmentRegions_WorkloadId')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_WorkloadEnvironmentRegions_WorkloadId 
        ON WorkloadEnvironmentRegions(WorkloadId);
    END
    
    -- Junction table relationship
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Resources_WorkloadEnvRegion')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Resources_WorkloadEnvRegion 
        ON Resources(WorkloadEnvironmentRegionId);
    END

    -- For filtering resources by type
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Resources_ResourceTypeId')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Resources_ResourceTypeId 
        ON Resources(ResourceTypeId) 
        INCLUDE (Name, StatusId);
    END

    -- For status checks
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Resources_StatusId')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Resources_StatusId 
        ON Resources(StatusId) 
        WHERE StatusId <> 3; -- Exclude 'Ready' status
    END

    -- ResourceTypes by category (for dropdowns)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResourceTypes_CategoryId')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_ResourceTypes_CategoryId 
        ON ResourceTypes(CategoryId)
        INCLUDE (Name, AzureResourceType);
    END

    -- Case-insensitive search on Azure types
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResourceTypes_AzureType')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_ResourceTypes_AzureType 
        ON ResourceTypes(AzureResourceType);
    END

    -- Fast environment/region filtering
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkloadEnvironmentRegions_Composite')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_WorkloadEnvironmentRegions_Composite 
        ON WorkloadEnvironmentRegions(EnvironmentTypeId, RegionId);
    END

    -- For the resource overview query you provided
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Resource_Overview')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Resource_Overview 
        ON Resources(WorkloadEnvironmentRegionId, ResourceTypeId, StatusId)
        INCLUDE (Name);
    END

COMMIT TRANSACTION;

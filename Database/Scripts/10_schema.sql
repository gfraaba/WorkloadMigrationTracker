USE WorkloadMigration;
GO

CREATE TABLE Workloads (
    WorkloadId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    AzureNamePrefix NVARCHAR(50) NOT NULL,
    PrimaryPOC NVARCHAR(100),
    SecondaryPOC NVARCHAR(100)
);

CREATE TABLE Resources (
    ResourceId INT IDENTITY(1,1) PRIMARY KEY,
    WorkloadId INT FOREIGN KEY REFERENCES Workloads(WorkloadId),
    Type NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Unavailable', 'Deployed', 'Ready')),
    RG NVARCHAR(100) NOT NULL
);

-- Environment Types
CREATE TABLE EnvironmentTypes (
    EnvironmentTypeId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

-- Azure Regions
CREATE TABLE AzureRegions (
    RegionId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,  -- e.g. "eastus"
    Name NVARCHAR(100) NOT NULL,        -- e.g. "East US"
    IsActive BIT DEFAULT 1
);

-- Resource Categories (Compute, Storage, etc.)
CREATE TABLE ResourceCategories (
    CategoryId INT PRIMARY KEY,  -- Explicit IDs
    Name NVARCHAR(50) NOT NULL UNIQUE,
    AzureServiceType NVARCHAR(100)  -- Maps to Azure's internal categories
);

-- Resource Types (VM, Storage Account, etc.)
CREATE TABLE ResourceTypes (
    TypeId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT FOREIGN KEY REFERENCES ResourceCategories(CategoryId),
    Name NVARCHAR(100) NOT NULL,
    AzureResourceType NVARCHAR(100) NOT NULL,  -- e.g. "Microsoft.Compute/virtualMachines"
    UNIQUE (CategoryId, Name)
);

-- A junction table for Workload-Environment-Region mapping
CREATE TABLE WorkloadEnvironmentRegions (
    WorkloadEnvironmentRegionId INT IDENTITY(1,1) PRIMARY KEY,
    WorkloadId INT NOT NULL FOREIGN KEY REFERENCES Workloads(WorkloadId),
    EnvironmentTypeId INT NOT NULL FOREIGN KEY REFERENCES EnvironmentTypes(EnvironmentTypeId),
    RegionId INT NOT NULL FOREIGN KEY REFERENCES AzureRegions(RegionId),
    AzureSubscriptionId NVARCHAR(100),
    ResourceGroupName NVARCHAR(100),
    UNIQUE (WorkloadId, EnvironmentTypeId, RegionId)
);

-- Resource Statuses
CREATE TABLE ResourceStatuses (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(20) NOT NULL UNIQUE  -- "Unavailable", "Deployed", "Ready"
);

ALTER TABLE Resources
ADD WorkloadEnvironmentRegionId INT NOT NULL FOREIGN KEY REFERENCES WorkloadEnvironmentRegions(WorkloadEnvironmentRegionId),
    ResourceTypeId INT NOT NULL FOREIGN KEY REFERENCES ResourceTypes(TypeId),
    StatusId INT NOT NULL FOREIGN KEY REFERENCES ResourceStatuses(StatusId);

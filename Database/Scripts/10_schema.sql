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
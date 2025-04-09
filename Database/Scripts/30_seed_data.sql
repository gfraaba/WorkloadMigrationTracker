USE WorkloadMigration;
GO

-- Environment Types
INSERT INTO EnvironmentTypes (Name, Description)
VALUES 
    ('Dev', 'Development Environment'),
    ('QA', 'Quality Assurance Environment'),
    ('Prod', 'Production Environment');

-- Azure Regions
INSERT INTO AzureRegions (Code, Name)
VALUES
    ('eastus', 'East US'),
    ('westus', 'West US');

-- Resource Categories with explicit IDs
INSERT INTO ResourceCategories (CategoryId, Name, AzureServiceType)
VALUES 
    (1, 'Compute', 'Microsoft.Compute'),
    (2, 'Storage', 'Microsoft.Storage'),
    (3, 'Network', 'Microsoft.Network'),
    (4, 'Database', 'Microsoft.Database'),
    (5, 'Monitoring', 'Microsoft.Insights'),
    (6, 'Security', 'Microsoft.Security');

-- Resource Statuses
INSERT INTO ResourceStatuses (Name)
VALUES
    ('Unavailable'),
    ('Deployed'),
    ('Ready');

-- Resource Types
INSERT INTO ResourceTypes (CategoryId, Name, AzureResourceType)
VALUES
    -- Compute
    (1, 'Virtual Machine', 'Microsoft.Compute/virtualMachines'),
    (1, 'VM Scale Set', 'Microsoft.Compute/virtualMachineScaleSets'),
    (1, 'App Service', 'Microsoft.Web/sites'),
    (1, 'Function App', 'Microsoft.Web/sites/functions'),
    (1, 'Container Instance', 'Microsoft.ContainerInstance/containerGroups'),
    (1, 'Kubernetes Service', 'Microsoft.ContainerService/managedClusters'),
    
    -- Storage
    (2, 'Storage Account', 'Microsoft.Storage/storageAccounts'),
    (2, 'Blob Container', 'Microsoft.Storage/storageAccounts/blobServices/containers'),
    (2, 'File Share', 'Microsoft.Storage/storageAccounts/fileServices/shares'),
    (2, 'Queue', 'Microsoft.Storage/storageAccounts/queueServices/queues'),
    (2, 'Table', 'Microsoft.Storage/storageAccounts/tableServices/tables'),
    (2, 'Disk', 'Microsoft.Compute/disks'),
    
    -- Networking
    (3, 'Virtual Network', 'Microsoft.Network/virtualNetworks'),
    (3, 'Subnet', 'Microsoft.Network/virtualNetworks/subnets'),
    (3, 'Network Security Group', 'Microsoft.Network/networkSecurityGroups'),
    (3, 'Load Balancer', 'Microsoft.Network/loadBalancers'),
    (3, 'Application Gateway', 'Microsoft.Network/applicationGateways'),
    (3, 'Front Door', 'Microsoft.Network/frontDoors'),
    
    -- Databases
    (4, 'SQL Database', 'Microsoft.Sql/servers/databases'),
    (4, 'Cosmos DB', 'Microsoft.DocumentDB/databaseAccounts'),
    (4, 'Redis Cache', 'Microsoft.Cache/Redis'),
    
    -- Monitoring
    (5, 'Log Analytics Workspace', 'Microsoft.OperationalInsights/workspaces'),
    (5, 'Application Insights', 'Microsoft.Insights/components'),
    
    -- Security
    (6, 'Key Vault', 'Microsoft.KeyVault/vaults'),
    (6, 'Managed Identity', 'Microsoft.ManagedIdentity/userAssignedIdentities');
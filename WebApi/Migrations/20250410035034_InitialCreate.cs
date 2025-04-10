using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AzureRegions",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureRegions", x => x.RegionId);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentTypes",
                columns: table => new
                {
                    EnvironmentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentTypes", x => x.EnvironmentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AzureServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Workloads",
                columns: table => new
                {
                    WorkloadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AzureNamePrefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryPOC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryPOC = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workloads", x => x.WorkloadId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceTypes",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AzureResourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceTypes", x => x.TypeId);
                    table.ForeignKey(
                        name: "FK_ResourceTypes_ResourceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ResourceCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkloadEnvironmentRegions",
                columns: table => new
                {
                    WorkloadEnvironmentRegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AzureSubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkloadId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentTypeId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkloadEnvironmentRegions", x => x.WorkloadEnvironmentRegionId);
                    table.ForeignKey(
                        name: "FK_WorkloadEnvironmentRegions_AzureRegions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "AzureRegions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkloadEnvironmentRegions_EnvironmentTypes_EnvironmentTypeId",
                        column: x => x.EnvironmentTypeId,
                        principalTable: "EnvironmentTypes",
                        principalColumn: "EnvironmentTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkloadEnvironmentRegions_Workloads_WorkloadId",
                        column: x => x.WorkloadId,
                        principalTable: "Workloads",
                        principalColumn: "WorkloadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkloadId = table.Column<int>(type: "int", nullable: false),
                    WorkloadEnvironmentRegionId = table.Column<int>(type: "int", nullable: false),
                    ResourceTypeId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                    table.ForeignKey(
                        name: "FK_Resources_ResourceStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ResourceStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_ResourceTypes_ResourceTypeId",
                        column: x => x.ResourceTypeId,
                        principalTable: "ResourceTypes",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_WorkloadEnvironmentRegions_WorkloadEnvironmentRegionId",
                        column: x => x.WorkloadEnvironmentRegionId,
                        principalTable: "WorkloadEnvironmentRegions",
                        principalColumn: "WorkloadEnvironmentRegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_Workloads_WorkloadId",
                        column: x => x.WorkloadId,
                        principalTable: "Workloads",
                        principalColumn: "WorkloadId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AzureRegions",
                columns: new[] { "RegionId", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "eastus", true, "East US" },
                    { 2, "westus", true, "West US" }
                });

            migrationBuilder.InsertData(
                table: "EnvironmentTypes",
                columns: new[] { "EnvironmentTypeId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Development Environment", "Dev" },
                    { 2, "Quality Assurance Environment", "QA" },
                    { 3, "Production Environment", "Prod" }
                });

            migrationBuilder.InsertData(
                table: "ResourceCategories",
                columns: new[] { "CategoryId", "AzureServiceType", "Name" },
                values: new object[,]
                {
                    { 1, "Microsoft.Compute", "Compute" },
                    { 2, "Microsoft.Storage", "Storage" },
                    { 3, "Microsoft.Network", "Network" },
                    { 4, "Microsoft.Database", "Database" },
                    { 5, "Microsoft.Insights", "Monitoring" },
                    { 6, "Microsoft.Security", "Security" }
                });

            migrationBuilder.InsertData(
                table: "ResourceStatuses",
                columns: new[] { "StatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Unavailable" },
                    { 2, "Deployed" },
                    { 3, "Ready" }
                });

            migrationBuilder.InsertData(
                table: "ResourceTypes",
                columns: new[] { "TypeId", "AzureResourceType", "CategoryId", "Name" },
                values: new object[,]
                {
                    { 1, "Microsoft.Compute/virtualMachines", 1, "Virtual Machine" },
                    { 2, "Microsoft.Compute/virtualMachineScaleSets", 1, "VM Scale Set" },
                    { 3, "Microsoft.Web/sites", 1, "App Service" },
                    { 4, "Microsoft.Web/sites/functions", 1, "Function App" },
                    { 5, "Microsoft.ContainerInstance/containerGroups", 1, "Container Instance" },
                    { 6, "Microsoft.ContainerService/managedClusters", 1, "Kubernetes Service" },
                    { 7, "Microsoft.Storage/storageAccounts", 2, "Storage Account" },
                    { 8, "Microsoft.Storage/storageAccounts/blobServices/containers", 2, "Blob Container" },
                    { 9, "Microsoft.Storage/storageAccounts/fileServices/shares", 2, "File Share" },
                    { 10, "Microsoft.Storage/storageAccounts/queueServices/queues", 2, "Queue" },
                    { 11, "Microsoft.Storage/storageAccounts/tableServices/tables", 2, "Table" },
                    { 12, "Microsoft.Compute/disks", 2, "Disk" },
                    { 13, "Microsoft.Network/virtualNetworks", 3, "Virtual Network" },
                    { 14, "Microsoft.Network/virtualNetworks/subnets", 3, "Subnet" },
                    { 15, "Microsoft.Network/networkSecurityGroups", 3, "Network Security Group" },
                    { 16, "Microsoft.Network/loadBalancers", 3, "Load Balancer" },
                    { 17, "Microsoft.Network/applicationGateways", 3, "Application Gateway" },
                    { 18, "Microsoft.Network/frontDoors", 3, "Front Door" },
                    { 19, "Microsoft.Sql/servers/databases", 4, "SQL Database" },
                    { 20, "Microsoft.DocumentDB/databaseAccounts", 4, "Cosmos DB" },
                    { 21, "Microsoft.Cache/Redis", 4, "Redis Cache" },
                    { 22, "Microsoft.OperationalInsights/workspaces", 5, "Log Analytics Workspace" },
                    { 23, "Microsoft.Insights/components", 5, "Application Insights" },
                    { 24, "Microsoft.KeyVault/vaults", 6, "Key Vault" },
                    { 25, "Microsoft.ManagedIdentity/userAssignedIdentities", 6, "Managed Identity" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AzureRegions_Code",
                table: "AzureRegions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_Name",
                table: "ResourceCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceTypeId",
                table: "Resources",
                column: "ResourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_StatusId",
                table: "Resources",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_WorkloadEnvironmentRegionId",
                table: "Resources",
                column: "WorkloadEnvironmentRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_WorkloadId",
                table: "Resources",
                column: "WorkloadId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceTypes_CategoryId_Name",
                table: "ResourceTypes",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadEnvironmentRegions_EnvironmentTypeId",
                table: "WorkloadEnvironmentRegions",
                column: "EnvironmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadEnvironmentRegions_RegionId",
                table: "WorkloadEnvironmentRegions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadEnvironmentRegions_WorkloadId_EnvironmentTypeId_RegionId",
                table: "WorkloadEnvironmentRegions",
                columns: new[] { "WorkloadId", "EnvironmentTypeId", "RegionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "ResourceStatuses");

            migrationBuilder.DropTable(
                name: "ResourceTypes");

            migrationBuilder.DropTable(
                name: "WorkloadEnvironmentRegions");

            migrationBuilder.DropTable(
                name: "ResourceCategories");

            migrationBuilder.DropTable(
                name: "AzureRegions");

            migrationBuilder.DropTable(
                name: "EnvironmentTypes");

            migrationBuilder.DropTable(
                name: "Workloads");
        }
    }
}

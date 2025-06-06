﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApi.Data;

#nullable disable

namespace WebApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250410035034_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebApi.Models.AzureRegion", b =>
                {
                    b.Property<int>("RegionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RegionId"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RegionId");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("AzureRegions");

                    b.HasData(
                        new
                        {
                            RegionId = 1,
                            Code = "eastus",
                            IsActive = true,
                            Name = "East US"
                        },
                        new
                        {
                            RegionId = 2,
                            Code = "westus",
                            IsActive = true,
                            Name = "West US"
                        });
                });

            modelBuilder.Entity("WebApi.Models.EnvironmentType", b =>
                {
                    b.Property<int>("EnvironmentTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EnvironmentTypeId"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EnvironmentTypeId");

                    b.ToTable("EnvironmentTypes");

                    b.HasData(
                        new
                        {
                            EnvironmentTypeId = 1,
                            Description = "Development Environment",
                            Name = "Dev"
                        },
                        new
                        {
                            EnvironmentTypeId = 2,
                            Description = "Quality Assurance Environment",
                            Name = "QA"
                        },
                        new
                        {
                            EnvironmentTypeId = 3,
                            Description = "Production Environment",
                            Name = "Prod"
                        });
                });

            modelBuilder.Entity("WebApi.Models.Resource", b =>
                {
                    b.Property<int>("ResourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ResourceId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ResourceTypeId")
                        .HasColumnType("int");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<int>("WorkloadEnvironmentRegionId")
                        .HasColumnType("int");

                    b.Property<int>("WorkloadId")
                        .HasColumnType("int");

                    b.HasKey("ResourceId");

                    b.HasIndex("ResourceTypeId");

                    b.HasIndex("StatusId");

                    b.HasIndex("WorkloadEnvironmentRegionId");

                    b.HasIndex("WorkloadId");

                    b.ToTable("Resources");
                });

            modelBuilder.Entity("WebApi.Models.ResourceCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("AzureServiceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("CategoryId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ResourceCategories");

                    b.HasData(
                        new
                        {
                            CategoryId = 1,
                            AzureServiceType = "Microsoft.Compute",
                            Name = "Compute"
                        },
                        new
                        {
                            CategoryId = 2,
                            AzureServiceType = "Microsoft.Storage",
                            Name = "Storage"
                        },
                        new
                        {
                            CategoryId = 3,
                            AzureServiceType = "Microsoft.Network",
                            Name = "Network"
                        },
                        new
                        {
                            CategoryId = 4,
                            AzureServiceType = "Microsoft.Database",
                            Name = "Database"
                        },
                        new
                        {
                            CategoryId = 5,
                            AzureServiceType = "Microsoft.Insights",
                            Name = "Monitoring"
                        },
                        new
                        {
                            CategoryId = 6,
                            AzureServiceType = "Microsoft.Security",
                            Name = "Security"
                        });
                });

            modelBuilder.Entity("WebApi.Models.ResourceStatus", b =>
                {
                    b.Property<int>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StatusId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StatusId");

                    b.ToTable("ResourceStatuses");

                    b.HasData(
                        new
                        {
                            StatusId = 1,
                            Name = "Unavailable"
                        },
                        new
                        {
                            StatusId = 2,
                            Name = "Deployed"
                        },
                        new
                        {
                            StatusId = 3,
                            Name = "Ready"
                        });
                });

            modelBuilder.Entity("WebApi.Models.ResourceType", b =>
                {
                    b.Property<int>("TypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TypeId"));

                    b.Property<string>("AzureResourceType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("TypeId");

                    b.HasIndex("CategoryId", "Name")
                        .IsUnique();

                    b.ToTable("ResourceTypes");

                    b.HasData(
                        new
                        {
                            TypeId = 1,
                            AzureResourceType = "Microsoft.Compute/virtualMachines",
                            CategoryId = 1,
                            Name = "Virtual Machine"
                        },
                        new
                        {
                            TypeId = 2,
                            AzureResourceType = "Microsoft.Compute/virtualMachineScaleSets",
                            CategoryId = 1,
                            Name = "VM Scale Set"
                        },
                        new
                        {
                            TypeId = 3,
                            AzureResourceType = "Microsoft.Web/sites",
                            CategoryId = 1,
                            Name = "App Service"
                        },
                        new
                        {
                            TypeId = 4,
                            AzureResourceType = "Microsoft.Web/sites/functions",
                            CategoryId = 1,
                            Name = "Function App"
                        },
                        new
                        {
                            TypeId = 5,
                            AzureResourceType = "Microsoft.ContainerInstance/containerGroups",
                            CategoryId = 1,
                            Name = "Container Instance"
                        },
                        new
                        {
                            TypeId = 6,
                            AzureResourceType = "Microsoft.ContainerService/managedClusters",
                            CategoryId = 1,
                            Name = "Kubernetes Service"
                        },
                        new
                        {
                            TypeId = 7,
                            AzureResourceType = "Microsoft.Storage/storageAccounts",
                            CategoryId = 2,
                            Name = "Storage Account"
                        },
                        new
                        {
                            TypeId = 8,
                            AzureResourceType = "Microsoft.Storage/storageAccounts/blobServices/containers",
                            CategoryId = 2,
                            Name = "Blob Container"
                        },
                        new
                        {
                            TypeId = 9,
                            AzureResourceType = "Microsoft.Storage/storageAccounts/fileServices/shares",
                            CategoryId = 2,
                            Name = "File Share"
                        },
                        new
                        {
                            TypeId = 10,
                            AzureResourceType = "Microsoft.Storage/storageAccounts/queueServices/queues",
                            CategoryId = 2,
                            Name = "Queue"
                        },
                        new
                        {
                            TypeId = 11,
                            AzureResourceType = "Microsoft.Storage/storageAccounts/tableServices/tables",
                            CategoryId = 2,
                            Name = "Table"
                        },
                        new
                        {
                            TypeId = 12,
                            AzureResourceType = "Microsoft.Compute/disks",
                            CategoryId = 2,
                            Name = "Disk"
                        },
                        new
                        {
                            TypeId = 13,
                            AzureResourceType = "Microsoft.Network/virtualNetworks",
                            CategoryId = 3,
                            Name = "Virtual Network"
                        },
                        new
                        {
                            TypeId = 14,
                            AzureResourceType = "Microsoft.Network/virtualNetworks/subnets",
                            CategoryId = 3,
                            Name = "Subnet"
                        },
                        new
                        {
                            TypeId = 15,
                            AzureResourceType = "Microsoft.Network/networkSecurityGroups",
                            CategoryId = 3,
                            Name = "Network Security Group"
                        },
                        new
                        {
                            TypeId = 16,
                            AzureResourceType = "Microsoft.Network/loadBalancers",
                            CategoryId = 3,
                            Name = "Load Balancer"
                        },
                        new
                        {
                            TypeId = 17,
                            AzureResourceType = "Microsoft.Network/applicationGateways",
                            CategoryId = 3,
                            Name = "Application Gateway"
                        },
                        new
                        {
                            TypeId = 18,
                            AzureResourceType = "Microsoft.Network/frontDoors",
                            CategoryId = 3,
                            Name = "Front Door"
                        },
                        new
                        {
                            TypeId = 19,
                            AzureResourceType = "Microsoft.Sql/servers/databases",
                            CategoryId = 4,
                            Name = "SQL Database"
                        },
                        new
                        {
                            TypeId = 20,
                            AzureResourceType = "Microsoft.DocumentDB/databaseAccounts",
                            CategoryId = 4,
                            Name = "Cosmos DB"
                        },
                        new
                        {
                            TypeId = 21,
                            AzureResourceType = "Microsoft.Cache/Redis",
                            CategoryId = 4,
                            Name = "Redis Cache"
                        },
                        new
                        {
                            TypeId = 22,
                            AzureResourceType = "Microsoft.OperationalInsights/workspaces",
                            CategoryId = 5,
                            Name = "Log Analytics Workspace"
                        },
                        new
                        {
                            TypeId = 23,
                            AzureResourceType = "Microsoft.Insights/components",
                            CategoryId = 5,
                            Name = "Application Insights"
                        },
                        new
                        {
                            TypeId = 24,
                            AzureResourceType = "Microsoft.KeyVault/vaults",
                            CategoryId = 6,
                            Name = "Key Vault"
                        },
                        new
                        {
                            TypeId = 25,
                            AzureResourceType = "Microsoft.ManagedIdentity/userAssignedIdentities",
                            CategoryId = 6,
                            Name = "Managed Identity"
                        });
                });

            modelBuilder.Entity("WebApi.Models.Workload", b =>
                {
                    b.Property<int>("WorkloadId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WorkloadId"));

                    b.Property<string>("AzureNamePrefix")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimaryPOC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecondaryPOC")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("WorkloadId");

                    b.ToTable("Workloads");
                });

            modelBuilder.Entity("WebApi.Models.WorkloadEnvironmentRegion", b =>
                {
                    b.Property<int>("WorkloadEnvironmentRegionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WorkloadEnvironmentRegionId"));

                    b.Property<string>("AzureSubscriptionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EnvironmentTypeId")
                        .HasColumnType("int");

                    b.Property<int>("RegionId")
                        .HasColumnType("int");

                    b.Property<string>("ResourceGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("WorkloadId")
                        .HasColumnType("int");

                    b.HasKey("WorkloadEnvironmentRegionId");

                    b.HasIndex("EnvironmentTypeId");

                    b.HasIndex("RegionId");

                    b.HasIndex("WorkloadId", "EnvironmentTypeId", "RegionId")
                        .IsUnique();

                    b.ToTable("WorkloadEnvironmentRegions");
                });

            modelBuilder.Entity("WebApi.Models.Resource", b =>
                {
                    b.HasOne("WebApi.Models.ResourceType", "ResourceType")
                        .WithMany("Resources")
                        .HasForeignKey("ResourceTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WebApi.Models.ResourceStatus", "Status")
                        .WithMany("Resources")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WebApi.Models.WorkloadEnvironmentRegion", "WorkloadEnvironmentRegion")
                        .WithMany("Resources")
                        .HasForeignKey("WorkloadEnvironmentRegionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WebApi.Models.Workload", "Workload")
                        .WithMany("Resources")
                        .HasForeignKey("WorkloadId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ResourceType");

                    b.Navigation("Status");

                    b.Navigation("Workload");

                    b.Navigation("WorkloadEnvironmentRegion");
                });

            modelBuilder.Entity("WebApi.Models.ResourceType", b =>
                {
                    b.HasOne("WebApi.Models.ResourceCategory", "Category")
                        .WithMany("ResourceTypes")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("WebApi.Models.WorkloadEnvironmentRegion", b =>
                {
                    b.HasOne("WebApi.Models.EnvironmentType", "EnvironmentType")
                        .WithMany()
                        .HasForeignKey("EnvironmentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApi.Models.AzureRegion", "Region")
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApi.Models.Workload", "Workload")
                        .WithMany("WorkloadEnvironmentRegions")
                        .HasForeignKey("WorkloadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EnvironmentType");

                    b.Navigation("Region");

                    b.Navigation("Workload");
                });

            modelBuilder.Entity("WebApi.Models.ResourceCategory", b =>
                {
                    b.Navigation("ResourceTypes");
                });

            modelBuilder.Entity("WebApi.Models.ResourceStatus", b =>
                {
                    b.Navigation("Resources");
                });

            modelBuilder.Entity("WebApi.Models.ResourceType", b =>
                {
                    b.Navigation("Resources");
                });

            modelBuilder.Entity("WebApi.Models.Workload", b =>
                {
                    b.Navigation("Resources");

                    b.Navigation("WorkloadEnvironmentRegions");
                });

            modelBuilder.Entity("WebApi.Models.WorkloadEnvironmentRegion", b =>
                {
                    b.Navigation("Resources");
                });
#pragma warning restore 612, 618
        }
    }
}

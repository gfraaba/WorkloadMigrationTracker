<#
.SYNOPSIS
    Creates the folder structure for a .NET Core WebAPI project for Workload Migration Tracker application.
.DESCRIPTION
    This script creates the folder structure and basic files for a WebAPI project
    following the specified architecture for the Workload Migration system.
#>

# Set the root directory for the project
$rootDir = "WebApi"
$projectDirs = @(
    "Controllers",
    "Models",
    "Data"
)

# Create subdirectories
foreach ($dir in $projectDirs) {
    New-Item -ItemType Directory -Path "$rootDir\$dir" -Force
}

# Create empty files for models
$modelFiles = @(
    "Workload.cs",
    "Resource.cs",
    "EnvironmentType.cs",
    "AzureRegion.cs",
    "ResourceCategory.cs",
    "ResourceType.cs",
    "WorkloadEnvironmentRegion.cs",
    "ResourceStatus.cs"
)

foreach ($file in $modelFiles) {
    New-Item -ItemType File -Path "$rootDir\Models\$file" -Force
}

# Create empty files for controllers
$controllerFiles = @(
    "WorkloadsController.cs",
    "ResourcesController.cs",
    "EnvironmentTypesController.cs",
    "AzureRegionsController.cs",
    "ResourceCategoriesController.cs",
    "ResourceTypesController.cs",
    "WorkloadEnvironmentRegionsController.cs",
    "ResourceStatusesController.cs"
)

foreach ($file in $controllerFiles) {
    New-Item -ItemType File -Path "$rootDir\Controllers\$file" -Force
}

# Create empty files for data
$dataFiles = @(
    "AppDbContext.cs",
    "DbInitializer.cs"
)

foreach ($file in $dataFiles) {
    New-Item -ItemType File -Path "$rootDir\Data\$file" -Force
}

# # Create a basic .NET solution file
# New-Item -ItemType File -Path "$rootDir\WorkloadMigrationApi.sln" -Force

Write-Host "Project structure created successfully at $((Get-Item $rootDir).FullName)"
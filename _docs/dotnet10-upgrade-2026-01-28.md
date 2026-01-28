# .NET 10 Upgrade Journal — 2026-01-28

This document records the .NET 10 upgrade work performed so far for WorkloadMigrationTracker: branches created, files changed, exact commands executed, build errors encountered, and fixes applied.

Summary
-------
- Branch for upgrade: `upgrade/dotnet-10` (created from `update/docker-images`).
- Objective: move projects from `net9.0` → `net10.0` and upgrade related NuGet packages (EF Core, OpenApi/OpenAPI packages, Swashbuckle, Blazor packages) while preserving devcontainer and DB compatibility.
- Status: Code updated and solution builds inside the `dev` container (after fixes). Remaining: run tests/migrations, validate runtime, open PR.

Branches & Commits
------------------
- `update/docker-images` (existing): Dockerfile + docs changes committed earlier.
- `upgrade/dotnet-10` (new): started from `update/docker-images` and used for TFM/package changes and build fixes.

Exact git commands used
----------------------
```bash
git checkout -B update/docker-images
git add -A
git commit -m "chore(docker): revert DB to 2022 for ARM64; add investigation doc"
git checkout -b upgrade/dotnet-10
```

Files Changed (summary)
-----------------------
- WebApi/WebApi.csproj
  - `TargetFramework` net9.0 → net10.0
  - Packages bumped: Microsoft.AspNetCore.OpenApi 9.0.4 → 10.0.2, Microsoft.EntityFrameworkCore 9.0.4 → 10.0.2, Microsoft.EntityFrameworkCore.Design 9.0.4 → 10.0.2, Microsoft.EntityFrameworkCore.SqlServer 9.0.4 → 10.0.2, Swashbuckle.AspNetCore 8.1.0 → 10.1.0, Microsoft.AspNetCore.Mvc.Core 2.3.0 → 2.3.9
  - Added/adjusted `Microsoft.OpenApi` package to resolve transitive constraints (final version used: 2.3.0)

- WebApp/WebApp.csproj
  - `TargetFramework` net9.0 → net10.0
  - Blazor package versions bumped to 10.0.x (settled on 10.0.2 for packages available on nuget)

- Shared/Shared.csproj
  - `TargetFramework` net9.0 → net10.0
  - `System.Text.Json` bumped to 10.x (pinned to 10.0.2)

- WebApi/Program.cs
  - Removed direct references to `Microsoft.OpenApi.Models` types used in Swagger setup; configured SwaggerGen to keep XML comments without strongly-typed OpenApi model construction to reduce version coupling.

- Database/db.Dockerfile and .devcontainer/docker-compose.yml (from prior investigation) — left as-is to keep local DB on SQL Server 2022 for ARM64 compatibility.

Exact dotnet / container commands executed (inside `dev` container)
-----------------------------------------------------------------
```bash
# Audit packages
cd /workspace
dotnet list WorkloadMigrationTracker.sln package --outdated
dotnet tool list -g

# Restore and build attempts during upgrade work
cd /workspace
dotnet restore
# or (when encountering transient network issues)
dotnet restore --disable-parallel --verbosity minimal

dotnet build WorkloadMigrationTracker.sln -c Debug --no-restore
```

Key errors encountered and fixes
--------------------------------
1. NU1102: Unable to find package `System.Text.Json` with version (>= 10.0.4)
   - Cause: requested a non-existent patch (10.0.4) at the time. Fix: pinned `System.Text.Json` to `10.0.2` (available on nuget.org).

2. Transient NuGet network/DNS error (Name or service not known for api.nuget.org)
   - Fix: re-run `dotnet restore --disable-parallel` (reduced parallelism) which succeeded.

3. CS0234: "The type or namespace name 'Models' does not exist in the namespace 'Microsoft.OpenApi'"
   - Cause: changes in package composition between `Microsoft.AspNetCore.OpenApi` and `Microsoft.OpenApi` versions.
   - Fix path(s) applied:
     - Added an explicit `Microsoft.OpenApi` package reference and adjusted its version to satisfy all transitive requirements.
     - Avoided direct usage of `Microsoft.OpenApi.Models` types in `Program.cs` (switched Swagger configuration to XML-comments-only approach where possible).
     - Iterated `Microsoft.OpenApi` version (1.3.0 → 2.0.0 → 2.3.0) to satisfy `Swashbuckle.AspNetCore 10.1.0` which requires Microsoft.OpenApi >= 2.3.0.

4. NU1605 / package-downgrade: surfaced while adding Microsoft.OpenApi; fixed by aligning `Microsoft.OpenApi` to required transitive version (2.3.0).

Build outcome after fixes
-------------------------
- `dotnet restore` succeeded.
- `dotnet build WorkloadMigrationTracker.sln -c Debug --no-restore` succeeded (final status: Build succeeded with warnings).
- Notable warnings: NU1510 (PackageReference may be unnecessary), and a few nullable warnings in `Program.cs` and controllers — to be addressed separately.

Notes about global tools
------------------------
- `dotnet-ef` inside the dev container is `10.0.2` (matches EF Core 10.x tooling).
- We did not modify global tools installation approach in the dev Dockerfile except ensuring `dotnet-ef` is available.

Why a separate branch?
----------------------
- Major TFM + package upgrades can introduce breaking API changes and require iterative fixes; a dedicated branch:
  - isolates the upgrade from other changes (docker/image fixes),
  - allows CI to validate the upgrade before merging, and
  - makes rollback straightforward if the upgrade causes regressions.

Remaining tasks to complete the upgrade
--------------------------------------
1. Run unit and integration tests.
2. Run EF Core migrations against the local dev DB (`Database` container with SQL Server 2022) and verify schema/seed behavior.
3. Address warnings (NU1510, nullable warnings) and run static analyzers.
4. Prepare a clear changelog and migration instructions (e.g., TFM bump, EF Core behavior notes).
5. Open PR `upgrade/dotnet-10` with commits, docs, and CI green checks.

Suggested next steps I can take now
----------------------------------
- Run integration tests and EF migrations inside the `dev` container and capture results in `_docs`.
- Open draft PR for `upgrade/dotnet-10` with the changes and the two `_docs` files attached.

---

If you want me to proceed, pick one:
- `run-tests`: run tests and migrations now, or
- `open-pr`: open a draft PR for `upgrade/dotnet-10` (I will push the branch and create PR).


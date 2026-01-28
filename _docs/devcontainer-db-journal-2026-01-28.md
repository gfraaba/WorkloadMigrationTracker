# Devcontainer / DB Investigation Journal — 2026-01-28

This is a detailed chronological journal capturing commands executed, outputs/errors observed, file edits performed, and decisions made while updating the devcontainer and database images and upgrading the repo to .NET 10.

---

## Overview
- Repo: WorkloadMigrationTracker
- Purpose: Refresh devcontainer + DB images, validate builds, audit NuGet packages, and upgrade projects to `net10.0`.
- Host: macOS (Apple Silicon)
- Date: 2026-01-28

---

## Environment / Important files inspected
- `.devcontainer/docker-compose.yml`
- `.devcontainer/.env` (contained `DOCKER_PLATFORM=linux/arm64`)
- `Database/db.Dockerfile`
- `.devcontainer/dev.Dockerfile`
- Solution and projects: `WorkloadMigrationTracker.sln`, `WebApi/`, `WebApp/`, `Shared/`

---

## Chronological Journal (commands, outputs, edits)

1) Initial compose build to see current state

Command:
```
docker compose -f .devcontainer/docker-compose.yml build --progress=plain
```
Outcome: Images built locally; subsequent `docker compose up` attempted to start services.


2) DB container failed immediately. Inspect logs.

Command:
```
docker compose -f .devcontainer/docker-compose.yml logs --no-color --no-log-prefix db --tail=200
```
Observed error in logs:
```
SQL Server 2025 will run as non-root by default.
This container is running as user mssql.
assertion failed [x86_avx_state_ptr->xsave_header.xfeatures == kSupportedXFeatureBits]: 
(ThreadContextSignals.cpp:414 rt_sigreturn)
```
Interpretation: The SQL Server 2025 binary hit an x86-specific AVX/XSave assertion when run on the environment the container used.


3) Confirm compose platform variable and local env

Command (repo search / read):
```
# Found in repo
cat .devcontainer/.env
# contents: DOCKER_PLATFORM=linux/arm64
```
Conclusion: compose was intentionally configured to build/use `linux/arm64` images. This previously worked with SQL Server 2022.


4) Inspect remote image manifests to compare 2022 vs 2025

Commands:
```
docker buildx imagetools inspect mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04 --raw
docker buildx imagetools inspect mcr.microsoft.com/mssql/server:2025-latest --raw
```
Outcome: Manifests were fetched.


5) Inspect local image platform

Command:
```
docker image inspect devcontainer-db --format '{{.Os}}/{{.Architecture}} {{.Config.Labels}}'
```
Result indicated the built image was `linux/arm64` when `.env` was `linux/arm64`.


6) Tentative workaround: try running DB under amd64 emulation

Action: edited `.devcontainer/docker-compose.yml` to force `db` `platform: linux/amd64` and added explanatory comments.

Patch (file changes):
- `.devcontainer/docker-compose.yml`: temporarily set `platform: linux/amd64` for `db` and added comment explaining SQL Server 2025 x86 requirements.

Build & start:
```
docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps db
```
Observed logs still showed the same AVX assertion (SQL Server 2025 failed under amd64 emulation on this host). Conclusion: QEMU emulation did not provide required CPU support here.


7) No-cache rebuild attempt to ensure not caching old layers

Commands used:
```
docker compose -f .devcontainer/docker-compose.yml build --no-cache db
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate db
```
Outcome: No change; runtime assertion persisted.


8) Decision: revert to the 2022 image which previously worked on ARM64

Edits:
- `Database/db.Dockerfile`: changed `FROM --platform=${PLATFORM} mcr.microsoft.com/mssql/server:2025-latest` → `mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04`
- `.devcontainer/docker-compose.yml`: restored `platform: ${DOCKER_PLATFORM}` and added a note that we reverted to 2022 for native ARM64 compatibility.

Commands to clean up and rebuild (one at a time):
```
docker compose -f .devcontainer/docker-compose.yml down --remove-orphans
docker rm -f devcontainer-db-1 || true
docker image rm -f devcontainer-db || true
docker compose -f .devcontainer/docker-compose.yml build db
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate db
docker compose -f .devcontainer/docker-compose.yml logs --no-color --no-log-prefix db --tail=200
```
Outcome: SQL Server 2022 started successfully (log shows SQL Server 2022 RTM-CU18 startup messages). Verified `dev` container started and used `linux/arm64`.


9) Document investigation

Action: created `_docs/devcontainer-db-investigation-2026-01-28.md` capturing timeline, commands, findings, and recommendations.


10) NuGet audit inside `dev` (run in container)

Commands run inside `dev` container:
```
cd /workspace
dotnet list WorkloadMigrationTracker.sln package --outdated
```
Observed output (WebApi project):
```
Project `WebApi` has the following updates to its packages
   [net9.0]: 
   Top-level Package                              Requested   Resolved   Latest
   > Microsoft.AspNetCore.Mvc.Core                2.3.0       2.3.0      2.3.9 
   > Microsoft.AspNetCore.OpenApi                 9.0.4       9.0.4      10.0.2
   > Microsoft.EntityFrameworkCore                9.0.4       9.0.4      10.0.2
   > Microsoft.EntityFrameworkCore.Design         9.0.4       9.0.4      10.0.2
   > Microsoft.EntityFrameworkCore.SqlServer      9.0.4       9.0.4      10.0.2
   > Swashbuckle.AspNetCore                       8.1.0       8.1.0      10.1.0
```

Also listed global tools:
```
dotnet tool list -g
# Output: dotnet-ef 10.0.2
```

Interpretation: EF Core 10.x and related 10.x packages exist; to upgrade them we must move projects to `net10.0` first.


11) Create branches and commit investigation changes

Commands executed:
```
git checkout -B update/docker-images
git add -A
git commit -m "chore(docker): revert DB to 2022 for ARM64; add investigation doc"
git checkout -b upgrade/dotnet-10
```
Outcome: `update/docker-images` contains the Dockerfile & docs changes; `upgrade/dotnet-10` created for the TFM/package work.


12) Prepare upgrade to .NET 10 (performed in `upgrade/dotnet-10` branch)

Files changed (TFM/package upgrades):
- `WebApi/WebApi.csproj`: `TargetFramework` net9.0 → net10.0, bumped packages: Microsoft.EntityFrameworkCore → 10.0.2, Microsoft.AspNetCore.OpenApi → 10.0.2, Swashbuckle.AspNetCore → 10.1.0, etc.
- `WebApp/WebApp.csproj`: net9.0 → net10.0, bumped Blazor packages to 10.x
- `Shared/Shared.csproj`: net9.0 → net10.0, bumped System.Text.Json to 10.0.x


13) Build & fixup in the dev container

Commands executed inside `dev` container (one-by-one):
```
cd /workspace
dotnet restore
dotnet build WorkloadMigrationTracker.sln -c Debug --no-restore
```
Initial errors encountered while attempting to upgrade:
- During an earlier run we saw a `NU1102` error for `System.Text.Json` 10.0.4 not found; fixed by pinning to 10.0.2 which is available on nuget.
- A compile-time error: Program used `Microsoft.OpenApi.Models` types; after upgrading `Microsoft.AspNetCore.OpenApi` and `Swashbuckle` there were transitive version constraints. Fixes applied:
  - Added an explicit `Microsoft.OpenApi` reference and adjusted its version to match transitive requirements (settled on `2.3.0`).
  - Avoided direct usage of `Microsoft.OpenApi.Models` types in `Program.cs` by using SwaggerGen configuration without direct type references where possible.

Key errors seen and how resolved (examples):
- NU1102: "Unable to find package System.Text.Json with version (>= 10.0.4)" → pinned to `10.0.2`.
- NuGet network/DNS transient error downloading a package ("Name or service not known (api.nuget.org:443)") → retry with `dotnet restore --disable-parallel` succeeded.
- CS0234: "The type or namespace name 'Models' does not exist in the namespace 'Microsoft.OpenApi'" → resolved by adding `Microsoft.OpenApi` package + matching versions or avoiding direct type usage.
- NU1605 / package downgrade warnings surfaced; resolved by selecting compatible `Microsoft.OpenApi` version (2.3.0) to satisfy Swashbuckle 10.x.

After fixes: `dotnet restore` succeeded and `dotnet build` succeeded (Build succeeded with warnings only).


14) Final verification

- `docker compose -f .devcontainer/docker-compose.yml up -d` → `db` (SQL Server 2022) and `dev` (arm64) run correctly.
- `dotnet build` inside `dev`: solution builds targeting `net10.0` with warnings only.

---

## Files edited (summary)
- `.devcontainer/docker-compose.yml` — temporarily forced `platform: linux/amd64` for `db` during investigation, then restored; added explanatory comments.
- `Database/db.Dockerfile` — reverted/changed base image: 2025 → 2022-CU18-ubuntu-22.04.
- `_docs/devcontainer-db-investigation-2026-01-28.md` — investigation report.
- `_docs/devcontainer-db-journal-2026-01-28.md` — this detailed journal.
- `WebApi/WebApi.csproj`, `WebApp/WebApp.csproj`, `Shared/Shared.csproj` — updated `TargetFramework` to `net10.0` and bumped package versions (with adjustments to match available NuGet versions).
- `WebApi/Program.cs` — removed direct `Microsoft.OpenApi.Models` type usage in Swagger setup to avoid compile-time issues; adjusted using directives.

---

## Recommendations & Next Steps
1. Open a PR for `update/docker-images` (already committed) to document the DB revert and include `_docs` findings.
2. Continue the `upgrade/dotnet-10` branch work with care:
   - Run unit/integration tests and EF migrations against the (local) SQL Server 2022 dev DB to validate runtime behavior.
   - Review `NU` warnings (NU1510) and consider removing unnecessary direct references (e.g., `System.Text.Json` if only provided by framework).
   - Pin native OS packages (`msodbcsql18`, `mssql-tools18`) in `Database/db.Dockerfile` if reproducibility is desired.
3. If SQL Server 2025 is required in the future, provision an amd64 VM (local or cloud) and point dev containers at it rather than relying on QEMU emulation.

---

## Raw notable outputs captured
- SQL Server 2025 runtime assertion:
```
assertion failed [x86_avx_state_ptr->xsave_header.xfeatures == kSupportedXFeatureBits]: 
(ThreadContextSignals.cpp:414 rt_sigreturn)
```

- NuGet error example during upgrade attempt:
```
Unable to find package System.Text.Json with version (>= 10.0.4)
# or transient download DNS: Name or service not known (api.nuget.org:443)
```

- Dotnet build final status:
```
Build succeeded with 4 warning(s)
```

---

If you'd like, I will:
- open PRs for both branches (`update/docker-images` and `upgrade/dotnet-10`), including this journal and the investigation doc, or
- continue and run integration tests and migrations against local DB and fix issues surfaced.

Which should I do next?
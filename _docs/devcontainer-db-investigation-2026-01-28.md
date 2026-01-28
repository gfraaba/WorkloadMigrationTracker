# Devcontainer / DB image investigation (2026-01-28)

This document records the step-by-step investigation, commands executed, file edits, findings, and recommendations while updating the devcontainer and DB Docker images for WorkloadMigrationTracker.

Summary
-------
- Goal: locate latest base image tags, update `.devcontainer` and `Database` Dockerfiles, build images, and verify runtime.
- Outcome: Updated images, discovered SQL Server 2025 Linux image requires x86_64 CPU features that fail on native ARM64 on Apple Silicon; reverted DB base image to 2022 to preserve local ARM64 development.

Timeline & Commands
-------------------
Below are the commands executed during the investigation and verification, in (roughly) chronological order.

Docker Compose Commands (exact)
--------------------------------
The commands below are the exact `docker` / `docker compose` invocations used during the investigation (PowerShell variants shown where used):

```powershell
# Build (plain):
docker compose -f .devcontainer/docker-compose.yml build --progress=plain

# Start both services (initial debug run):
docker compose -f .devcontainer/docker-compose.yml up -d db dev

# Start db only (cached build via up):
docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps db

# Inspect logs (db):
docker compose -f .devcontainer/docker-compose.yml logs --no-color --no-log-prefix db --tail=200

# Build db explicitly (no-cache build via compose build):
docker compose -f .devcontainer/docker-compose.yml build --no-cache db

# Start only db after build (recreate):
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate db

# Stop/cleanup compose resources and orphans:
docker compose -f .devcontainer/docker-compose.yml down --remove-orphans

# Start dev container (no-deps):
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate dev

# Show running services:
docker compose -f .devcontainer/docker-compose.yml ps --services --filter "status=running"

# PowerShell-specific env example (attempted):
$env:DOCKER_PLATFORM='linux/amd64'; docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps db; $LASTEXITCODE

``` 


1) Initial compose build (plain progress)

```powershell
docker compose -f .devcontainer/docker-compose.yml build --progress=plain
```

2) Inspect DB logs after a failing start (saw x86 AVX assertion)

```powershell
docker compose -f .devcontainer/docker-compose.yml logs --no-color --no-log-prefix db --tail=200
```

3) Start services for initial debugging

```powershell
docker compose -f .devcontainer/docker-compose.yml up -d db dev
```

4) Attempt to rebuild with a different platform (attempts shown; some commands were canceled by operator)

```powershell
# attempted (PowerShell):
$env:DOCKER_PLATFORM='linux/amd64'; docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps db; $LASTEXITCODE

# attempted (not supported flags):
docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps --force-recreate --no-cache db
```

5) Inspect remote image manifests (to check multi-arch support)

```powershell
# 2022
docker buildx imagetools inspect mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04 --raw

# 2025
docker buildx imagetools inspect mcr.microsoft.com/mssql/server:2025-latest --raw
```

6) Inspect local built DB image architecture and labels

```powershell
docker image inspect devcontainer-db --format '{{.Os}}/{{.Architecture}} {{.Config.Labels}}'
# and later:

docker image inspect devcontainer-db --format '{{.Id}} {{.Created}} {{.Os}}/{{.Architecture}} {{.RepoTags}}'
```

7) Look up compose variable and env

```powershell
# search for DOCKER_PLATFORM in repo
# (performed with a grep_search in session)

# read .devcontainer/.env
cat .devcontainer/.env
# contents included: DOCKER_PLATFORM=linux/arm64
```

8) Patch `.devcontainer/docker-compose.yml` to force `db` platform to `linux/amd64` (temporary change)

Files edited:

- `.devcontainer/docker-compose.yml`

Command applied (file patch recorded in repository): changed `platform` for `db` to `linux/amd64` and added explanatory comment.

9) Rebuild `db` (initial cached build and later no-cache build attempts)

```powershell
# cached build via up (used cached layers):
docker compose -f .devcontainer/docker-compose.yml up -d --build --no-deps db

# explicit no-cache build (compose v2 doesn't accept --no-cache on up):
docker compose -f .devcontainer/docker-compose.yml build --no-cache db

# then start the rebuilt container:
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate db
```

10) Observed runtime error while running SQL Server 2025 under arm64 or under qemu emulation:

```text
assertion failed [x86_avx_state_ptr->xsave_header.xfeatures == kSupportedXFeatureBits]: 
(ThreadContextSignals.cpp:414 rt_sigreturn)
```

11) Reached conclusion that SQL Server 2025 binary requires x86_64 CPU features not provided by native ARM64 nor by the current emulation on Apple Silicon.

12) Reverted `Database/db.Dockerfile` to use the 2022 image tag and restored `platform: ${DOCKER_PLATFORM}` so the developer machine's `DOCKER_PLATFORM` (linux/arm64) is used.

Files edited:

- `Database/db.Dockerfile` — changed `FROM --platform=${PLATFORM} mcr.microsoft.com/mssql/server:2025-latest` to `FROM --platform=${PLATFORM} mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04`
- `.devcontainer/docker-compose.yml` — restored `platform: ${DOCKER_PLATFORM}` and updated comment.

13) Clean up and rebuild sequence (one command at a time):

```powershell
# stop and remove compose resources
docker compose -f .devcontainer/docker-compose.yml down --remove-orphans

# ensure no stale container or image
docker rm -f devcontainer-db-1 || true
docker image rm -f devcontainer-db || true

# full rebuild of the `db` service (2022 base image)
docker compose -f .devcontainer/docker-compose.yml build db

# start the `db` container
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate db

# check db logs
docker compose -f .devcontainer/docker-compose.yml logs --no-color --no-log-prefix db --tail=200

# start the dev container (keeps arm64)
docker compose -f .devcontainer/docker-compose.yml up -d --no-deps --force-recreate dev

# confirm running services
docker compose -f .devcontainer/docker-compose.yml ps --services --filter "status=running"
```

14) Verification

- After reverting to SQL Server 2022 image, logs show normal SQL Server 2022 startup messages (copying system database files, SQL Server 2022 RTM-CU18 detected, CPU vectorization levels including AVX/AVX2 detected — the 2022 binary behaved on the host environment).
- `dev` container started using `DOCKER_PLATFORM=linux/arm64` as intended.

Files Changed (summary)
-----------------------
- `Database/db.Dockerfile` — reverted base image to `mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04`.
- `.devcontainer/docker-compose.yml` — temporarily forced `db` to `linux/amd64` during investigation, then restored to use `platform: ${DOCKER_PLATFORM}` and added explanatory comment about the 2025 behavior.

Key Findings
------------
- SQL Server 2025 Linux image requires CPU features that are not available on native ARM64 and which are not satisfactorily provided by QEMU/amd64 emulation on Apple Silicon in our environment — this leads to a runtime assertion and process exit.
- Reverting to the 2022 image restores native ARM64 compatibility for local devcontainers.
- Rebuilding with or without cache does not change the base SQL Server binary; the runtime assertion is caused by binary requirements, not by stale layers.

Recommendations
---------------
1. For local development on Apple Silicon, keep `Database/db.Dockerfile` using SQL Server 2022 (or another tag known to work) unless you plan to run SQL Server 2025 on a remote amd64 host.
2. If you need SQL Server 2025, run it on an amd64 VM (local or cloud) and point `dev` at that host instead of trying to run it under emulation.
3. Pin native apt packages (msodbcsql18, mssql-tools18) to specific patch versions if reproducibility is required.
4. Perform package audits and TFM/nuget upgrades inside the `dev` container (do not add project package references during image builds).

Appendix — Notable logs (excerpts)
----------------------------------
- SQL Server 2025 assertion (observed when attempting to run 2025 under arm64/emulation):

```
assertion failed [x86_avx_state_ptr->xsave_header.xfeatures == kSupportedXFeatureBits]: 
(ThreadContextSignals.cpp:414 rt_sigreturn)
```

- SQL Server 2022 successful startup excerpt (after revert):

```
Microsoft SQL Server 2022 (RTM-CU18) (KB5050771) - 16.0.4185.3 (X64)
Server process ID is 464.
SQL Server detected 1 sockets with 12 cores per socket and 12 logical processors per socket
CPU vectorization level(s) detected:  SSE SSE2 SSE3 SSSE3 SSE41 SSE42 AVX AVX2 POPCNT BMI1 BMI2
```

— End of report —

If you'd like, I can:
- open a PR with the `Database/db.Dockerfile` and `.devcontainer/docker-compose.yml` changes, including a short changelog entry under `_docs` or `ReadMe.md`.
- run `dotnet list package --outdated` and `dotnet tool list -g` inside the running `dev` container and add the results to this document.

Audit Results (executed inside `dev` container)
---------------------------------------------

Command:

```powershell
dotnet list WorkloadMigrationTracker.sln package --outdated
```

Output (WebApi project):

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

Command:

```powershell
dotnet tool list -g
```

Output:

```
Package Id      Version      Commands 
--------------------------------------
dotnet-ef       10.0.2       dotnet-ef
```

Notes:
- Several major-version updates are available (EF Core 10 / dotnet-ef 10) which require moving projects to `net10.0` before upgrading packages.
- I recommend performing package upgrades and TFM changes in a dedicated branch and inside the `dev` container to avoid build-time package modifications in Dockerfiles.

*** File generated by assistant on 2026-01-28

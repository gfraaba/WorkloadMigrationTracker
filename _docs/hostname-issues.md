# Hostname, Docker, and SPA/API Mismatch — Deep Dive

This document explains the networking and hostname issues we encountered, why they happen, and how to remediate them. It is intentionally thorough so reviewers and future maintainers understand the trade-offs and history.

## Executive summary

- Symptom: Blazor SPA occasionally attempted to fetch API endpoints and received the SPA HTML shell (text/html) instead of JSON, causing System.Text.Json parse errors in the client. Headless Playwright scripts sometimes timed out waiting for DOM state because the client had not populated data.
- Root cause: environment-dependent origin/resolution differences between the SPA host and the API host. When the SPA makes a relative request (e.g. `/api/workloads`) it targets the SPA origin. If the SPA origin is not the API host (for example when the SPA is served by the app host but the API is on a different host/port), that request may hit the SPA server which returns the SPA shell HTML for unknown routes. The client then tries to parse HTML as JSON and fails.
- Not a single "who goofed up" — this is a common environment mismatch problem seen in many projects that separate SPA and API hosts, especially when running mixed host/container setups (host machine vs Docker container). The correct fixes are a combination of runtime configuration and defensive client behavior.

## Background: hosts, containers, and `host.docker.internal`

- `localhost` inside a container refers to the container itself. If you run the SPA in a container and the API in the host, `localhost` inside the container does NOT reach the host.
- `host.docker.internal` is a DNS alias Docker provides on many platforms to let a container reach the Docker host using a stable name. It is convenient but platform-dependent and brittle across environment types (macOS, Linux, Windows, different Docker providers).
- When you run everything on the host machine (not in containers), `localhost` is usually the correct address for both SPA and API.
- When running inside a devcontainer/docker-compose setup you may need explicit hostnames or mapped ports to ensure the SPA can reach the API.

Common permutations we must tolerate:
- Host-only run: SPA and API on host ports (e.g. SPA:5049, API:5005). `localhost` works.
- Container-only run: both SPA and API inside containers on the same Docker network. Containers can reach each other by service name.
- Mixed run: SPA in container, API on host. `host.docker.internal` or explicit mapped IP is needed.

Why this matters: relative fetches from the browser target the SPA's origin. If SPA and API origins are different, relative paths must be avoided or the SPA must be configured with the API base URL.

## What happened in this repo

- The SPA made relative calls to `/api/...`. In many developer workflows this worked fine because developers started SPA and API such that the origins matched or proxying was used.
- With headless scripts and devcontainer runs we sometimes ran the SPA origin (5049) and API origin (5005) differently. When Playwright requested a SPA route that attempted to fetch `/api/...`, the SPA origin served the SPA shell (as it does for unknown routes) instead of forwarding. The client then attempted JSON parsing and threw `ExpectedStartOfValueNotFound` or similar errors.
- Hard-coded or implicit hostnames like `host.docker.internal` increased the brittleness because they're environment-specific.

## Changes made here and rationale

1. Runtime-configurable API base:
   - We added a SPA runtime accessor `window.getTestApiBaseUrl()` (exposed via `index.html`) and standardized on the env variable name `TEST_API_BASE_URL`. This lets the SPA know an explicit API origin at runtime and avoid ambiguous relative fetches.

2. Defensive client fallback logic (`WorkloadService`):
   - When the client detects an HTML response or a non-JSON body for API endpoints, it retries the request against the explicit `TEST_API_BASE_URL` exposed by the SPA. This prevents the client from attempting to parse SPA HTML as JSON.

3. Headless tools and orchestration updated to accept `APP_BASE_URL` and `TEST_API_BASE_URL` env vars so they can run deterministically on host or inside containers.

4. Case-insensitive JSON parsing set for DTOs to reduce failures due to casing mismatches between server and client.

5. Increased robustness in headless Playwright scripts (longer waits, existence checks, directory safety) so automation is resilient to slower startup times.

## Who is at fault? Is this a design problem?

- This is a systems/deployment mismatch rather than a single developer "error." It stems from ambiguous assumptions about origin resolution:
  - The SPA assumed that a relative path `/api/*` would always reach the API endpoint that returns JSON. That assumption is valid when SPA and API share an origin or when a reverse proxy is present.
  - In mixed-run environments (containers vs host), that assumption is false unless explicitly configured.
- So it's a design gap in the SPA's runtime configuration strategy (no explicit API base by default), not a one-person mistake. The pragmatic fix is to make runtime configuration explicit and defensive behavior in the client.

## Recommended best practices (short and long term)

Short-term (what we implemented)
- Expose an explicit runtime `TEST_API_BASE_URL` (or `VITE_API_URL`/similar) from the SPA shell and use it for non-relative API calls.
- Add a client-side fallback when content-type is not JSON (treat HTML responses as a probable SPA shell and retry against the explicit API base).
- Make headless and orchestration scripts accept `APP_BASE_URL` and `TEST_API_BASE_URL` env vars and default sensibly to `localhost` for host runs.

Long-term / stronger designs
- Use a reverse proxy in front of SPA and API during local dev (e.g., dev server proxy) so the browser sees the same origin and relative paths work consistently.
- Make CI/devcontainer compose bring up a tiny nginx/proxy service that maps `/api` to the API container—this reproduces production behavior locally.
- Consider CORS and AllowedOrigins: prefer configuring ALLOWED_ORIGINS via env instead of hard-coding `host.docker.internal` in config files.
- Document the local development matrix clearly in `README.md` or an `_docs/run-matrix.md` so contributors know which hostnames to use in each mode.

## Debugging checklist for future failures

1. Reproduce: open browser console or Playwright logs and capture the failing request.
2. Inspect response body: if it contains the SPA HTML shell, the browser-targeted origin is wrong.
3. Check what the SPA's runtime `getTestApiBaseUrl()` returns (or absence thereof).
4. Verify whether the client sent a relative URL; if so, change to absolute using known API base.
5. Check Docker vs host run: container `localhost` differs from host `localhost`. Use `host.docker.internal` only if you understand the platform implications.
6. For CI/devcontainer: set `TEST_API_BASE_URL` explicitly in compose or launch scripts.

## Example remedial patterns

- In SPA code, prefer:

  - Use absolute base: `${apiBase}/api/workloads` where `apiBase` is the runtime value.
  - Avoid blind assumptions that `/api/...` resolves to the API server.

- In orchestration scripts:
  - Accept `TEST_API_BASE_URL` and `APP_BASE_URL` from the environment and emit them in startup logs so CI runs are auditable.

## Closing thoughts

This class of problem appears frequently when teams separate front-end and back-end hosts and then mix containerized and host-based workflows. The fix is predictable: make the API base explicit at runtime for the SPA, add small defensive retries in the client, and unify how test scripts pass environment values. The changes in this branch implement those pragmatic mitigations.

If you want, I can:
- Add a short `devcontainer/README.md` with exact env variables to set for each run mode.
- Add a small nginx reverse-proxy docker service to the compose file to make `/api` proxying consistent across local and container runs.


*** End of document

#!/usr/bin/env bash
set -euo pipefail

# Ensure we are running under Bash 4+ (macOS ships older bash by default).
# If the current shell is older than v4, try to re-exec with Homebrew bash
# which is commonly installed at /opt/homebrew/bin/bash on macOS ARM.
if [ -z "${BASH_VERSINFO:-}" ] || [ "${BASH_VERSINFO[0]:-0}" -lt 4 ]; then
  if [ -x "/opt/homebrew/bin/bash" ]; then
    echo "Re-execing with /opt/homebrew/bin/bash to enable Bash 4+ features"
    exec /opt/homebrew/bin/bash "$0" "$@"
  fi
  if [ -x "/usr/local/bin/bash" ]; then
    echo "Re-execing with /usr/local/bin/bash to enable Bash 4+ features"
    exec /usr/local/bin/bash "$0" "$@"
  fi
  echo "This script requires Bash 4+ (for mapfile and associative arrays)." >&2
  echo "Install Bash via Homebrew and run: /opt/homebrew/bin/bash $0" >&2
  exit 1
fi

# Recommended invocation to show live stdout while saving a log.
# If your default system `bash` is the older macOS /bin/bash, run explicitly
# with the Homebrew-installed bash (example for macOS ARM/Homebrew):
# 
# bash -lc "/opt/homebrew/bin/bash -lc 'chmod +x _scripts/setup_and_run_tests.sh; _scripts/setup_and_run_tests.sh' 2>&1 | tee _docs/tests/setup_and_run_tests.log"
# 
#   /opt/homebrew/bin/bash -lc "chmod +x _scripts/setup_and_run_tests.sh; _scripts/setup_and_run_tests.sh 2>&1 | tee _docs/tests/setup_and_run_tests.log"
#
# Or run and follow the log with `tee` (portable):
#
#   bash -lc "chmod +x _scripts/setup_and_run_tests.sh; _scripts/setup_and_run_tests.sh 2>&1 | tee _docs/tests/setup_and_run_tests.log"
#
# This runs the script normally but pipes both stdout and stderr to `tee`, which
# writes output to the console and also saves it to `_docs/tests/setup_and_run_tests.log`.
# Use `stdbuf -oL` if you encounter buffering issues from child processes:
#
#   stdbuf -oL _scripts/setup_and_run_tests.sh 2>&1 | tee _docs/tests/setup_and_run_tests.log
#
# Or run in background and follow the log:
#
#   nohup _scripts/setup_and_run_tests.sh > _docs/tests/setup_and_run_tests.log 2>&1 &
#   tail -f _docs/tests/setup_and_run_tests.log


# setup_and_run_tests.sh
# Starts a local SQL Server container (if not present), waits for readiness,
# restarts WebApi and WebApp using _scripts/restart_services.sh, then runs
# the Playwright workload-creator script. Logs are written under _docs/tests.

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOG_DIR="$ROOT/_docs/tests"
mkdir -p "$LOG_DIR"

# Configuration (can override via env)
DB_CONTAINER_NAME=${DB_CONTAINER_NAME:-wmt-db}
WEBAPP_PORT=${1:-5049}
API_PORT=${2:-5005}

# Source .devcontainer/.env or .env if present to pick up DB_PASSWORD
if [ -f "$ROOT/.devcontainer/.env" ]; then
  echo "Sourcing .devcontainer/.env"
  set -a
  # shellcheck disable=SC1090
  . "$ROOT/.devcontainer/.env"
  set +a
elif [ -f "$ROOT/.env" ]; then
  echo "Sourcing .env"
  set -a
  # shellcheck disable=SC1090
  . "$ROOT/.env"
  set +a
else
  echo "No .devcontainer/.env or .env file found; relying on environment variables"
fi

# Ensure DB and SA password alignment: prefer DB_PASSWORD from env files
if [ -z "${DB_PASSWORD:-}" ]; then
  echo "Error: DB_PASSWORD is not set. Please set DB_PASSWORD in .devcontainer/.env or .env or export it." >&2
  exit 1
fi

# Use the same password for SA_PASSWORD unless explicitly set
SA_PASSWORD=${SA_PASSWORD:-$DB_PASSWORD}
echo "Using DB_PASSWORD from env and SA_PASSWORD aligned (hidden). DB container name: $DB_CONTAINER_NAME"

start_db() {
  # Prefer using docker compose so service lifecycle is driven from docker-compose.yml
  # and environment substitution works consistently for dev. This avoids ad-hoc
  # 'docker run' calls and respects the compose configuration the repo already uses.
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    # Use an array so the two-word command 'docker compose' is invoked correctly
    DCMD=(docker compose)
  elif command -v docker-compose >/dev/null 2>&1; then
    DCMD=(docker-compose)
  else
    echo "Error: docker compose is not available. Install Docker Compose or use Docker Desktop." >&2
    exit 1
  fi

  # If any container is using port 1433, or any container is based on the
  # Microsoft SQL Server image, remove it to avoid port conflicts. Compose
  # may name containers differently (project/service index), so checking only
  # for $DB_CONTAINER_NAME can miss existing DB containers (e.g. devcontainer-db-1).
  echo "Checking for existing SQL Server containers or listeners on port 1433..."
  mapfile -t candidates < <(docker ps -a --format '{{.Names}} {{.Ports}}' | awk '/1433/ {print $1}') || true
  # Include containers that were created from the official mssql image
  mapfile -t mssql_containers < <(docker ps -a --filter ancestor=mcr.microsoft.com/mssql/server --format '{{.Names}}') || true
  for n in "${mssql_containers[@]:-}"; do
    candidates+=("$n")
  done
  # Make the list unique
  if [ ${#candidates[@]} -gt 0 ]; then
    declare -A seen
    uniq_candidates=()
    for c in "${candidates[@]}"; do
      if [ -n "$c" ] && [ -z "${seen[$c]:-}" ]; then
        seen[$c]=1
        uniq_candidates+=("$c")
      fi
    done
    if [ ${#uniq_candidates[@]} -gt 0 ]; then
      echo "Found existing SQL-related containers: ${uniq_candidates[*]}"
      # Try compose down for both compose files to clean up networks/volumes
      if [ -f "$ROOT/.devcontainer/docker-compose.yml" ]; then
        "${DCMD[@]}" -f "$ROOT/.devcontainer/docker-compose.yml" down --remove-orphans || true
      fi
      if [ -f "$ROOT/docker-compose.yml" ]; then
        "${DCMD[@]}" -f "$ROOT/docker-compose.yml" down --remove-orphans || true
      fi
      # Force remove any remaining matching containers
      for c in "${uniq_candidates[@]}"; do
        if docker ps -a --format '{{.Names}}' | grep -qx "$c"; then
          echo "Forcibly removing container '$c'"
          docker rm -f "$c" || true
        fi
      done
    fi
  fi

  # Choose the compose file to use. The repo includes a devcontainer compose
  # that lives in `.devcontainer/docker-compose.yml` where build contexts are
  # relative to the devcontainer directory. Prefer that file when present.
  if [ -f "$ROOT/.devcontainer/docker-compose.yml" ]; then
    COMPOSE_FILE="$ROOT/.devcontainer/docker-compose.yml"
  elif [ -f "$ROOT/docker-compose.yml" ]; then
    COMPOSE_FILE="$ROOT/docker-compose.yml"
  else
    echo "Error: no docker-compose.yml found in project (checked $ROOT/.devcontainer/docker-compose.yml and $ROOT/docker-compose.yml)" >&2
    exit 1
  fi

  echo "Using compose file: $COMPOSE_FILE"
  echo "Creating/starting DB service '$DB_CONTAINER_NAME' via: ${DCMD[*]} -f $COMPOSE_FILE up -d db"
  # Ensure environment variables we sourced earlier are exported for compose variable substitution
  "${DCMD[@]}" -f "$COMPOSE_FILE" up -d db

  # Detect the actual container name created for the DB service (compose may name it differently).
  detected_container=$(docker ps --filter ancestor=mcr.microsoft.com/mssql/server --format '{{.Names}}' | head -n1 || true)
  if [ -z "$detected_container" ]; then
    detected_container=$(docker ps --format '{{.Names}} {{.Ports}}' | awk '/1433/ {print $1; exit}' || true)
  fi
  if [ -n "$detected_container" ]; then
    echo "Detected DB container: $detected_container"
    DB_CONTAINER_NAME="$detected_container"
  else
    echo "Warning: could not detect DB container by image or port; using configured name: $DB_CONTAINER_NAME"
  fi

  echo "Waiting for SQL Server readiness (up to 180s)..."
  ready=0
  for i in $(seq 1 180); do
    # Fetch recent logs locally so we can debug if startup stalls
    docker logs "$DB_CONTAINER_NAME" 2>&1 | tail -n 200 > "$LOG_DIR/db_container_recent.log" || true
    if docker logs "$DB_CONTAINER_NAME" 2>&1 | tail -n 200 | grep -q 'SQL Server is now ready for client connections'; then
      ready=1
      echo "SQL Server ready after ${i}s"
      break
    fi
    sleep 1
  done
  # Persist full container logs for troubleshooting
  docker logs "$DB_CONTAINER_NAME" 2>&1 | sed -n '1,500p' > "$LOG_DIR/db_container.log" || true

  if [ $ready -eq 0 ]; then
    echo "ERROR: SQL Server not ready after 180s. See $LOG_DIR/db_container.log and $LOG_DIR/db_container_recent.log" >&2
    exit 1
  fi
}

run_restart() {
  echo "Running restart_services.sh (WebApi:$API_PORT WebApp:$WEBAPP_PORT)..."
  chmod +x "$ROOT/_scripts/restart_services.sh"
  "$ROOT/_scripts/restart_services.sh" "$WEBAPP_PORT" "$API_PORT" > "$LOG_DIR/restart_services_output.log" 2>&1 || true
  echo "Restart output saved to $LOG_DIR/restart_services_output.log"
}

run_playwright_create() {
  echo "Running Playwright workload creator..."
  # Use arm64 Playwright image when available on host to avoid cross-platform emulation.
  docker run --platform=linux/arm64 --rm -v "$ROOT:/workspace" -w /workspace mcr.microsoft.com/playwright:focal bash -lc \
    "node tools/headless-create-workload.js >/workspace/_docs/tests/headless_create_output.log 2>&1 || true; echo 'Playwright output saved to /workspace/_docs/tests/headless_create_output.log'"
}

echo "--- Starting DB ---"
start_db

echo "--- Restarting services ---"
run_restart

# Verify API database health before running Playwright
echo "Checking API database health (http://localhost:$API_PORT/api/health/database)"
HEALTH_DB_JSON="$LOG_DIR/health_db.json"
http_status=$(curl -sS -o "$HEALTH_DB_JSON" -w "%{http_code}" --max-time 5 "http://localhost:$API_PORT/api/health/database" || echo "000")
echo "API health probe HTTP status: $http_status"
if [ "$http_status" != "200" ]; then
  echo "ERROR: API health/database probe returned HTTP $http_status. See $HEALTH_DB_JSON" >&2
  cat "$HEALTH_DB_JSON" || true
  exit 1
fi
# Ensure response indicates connected database
if ! grep -q '"status"\s*:\s*"Healthy"' "$HEALTH_DB_JSON" || ! grep -q '"database"\s*:\s*"Connected"' "$HEALTH_DB_JSON"; then
  echo "ERROR: API health/database response did not indicate a connected DB. See $HEALTH_DB_JSON" >&2
  cat "$HEALTH_DB_JSON" || true
  exit 1
fi

echo "--- Running Playwright create-workload ---"
run_playwright_create

echo "Done. Artifacts and logs are under $LOG_DIR"

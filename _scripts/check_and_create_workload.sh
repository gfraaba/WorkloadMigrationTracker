#!/usr/bin/env bash
set -euo pipefail

# check_and_create_workload.sh [API_BASE]
# - API_BASE: e.g. http://host.docker.internal:5005 or http://localhost:5005
#
# Notes:
# - When you run this script on the host machine (macOS/Linux) and the WebApi
#   is started locally via `dotnet run` (listening on 5005), use `http://localhost:5005`.
# - When you run this script from inside a Docker container that needs to reach
#   services on the host, set `TEST_API_BASE_URL` to the appropriate host address
#   (e.g. `http://host.docker.internal:5005` on Docker Desktop). The script
#   prefers the `TEST_API_BASE_URL` environment variable when provided, otherwise
#   it defaults to `http://localhost:5005` for local host-based runs.
#
# Example (host):
#   _scripts/check_and_create_workload.sh http://localhost:5005
# Example (container):
#   TEST_API_BASE_URL=http://host.docker.internal:5005 _scripts/check_and_create_workload.sh


ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOG_DIR="$ROOT/_docs/tests"
mkdir -p "$LOG_DIR"

API_BASE=${1:-${TEST_API_BASE_URL:-http://localhost:5005}}
API_URL="$API_BASE/api/workloads"

OUT_JSON="$LOG_DIR/api_workloads_check.json"
POST_JSON="$LOG_DIR/api_workload_post.json"
OUT_LOG="$LOG_DIR/check_and_create_workload.log"

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Checking workloads at $API_URL" | tee "$OUT_LOG"

# Verify API database health before attempting workload checks/creation
HEALTH_DB_JSON="$LOG_DIR/health_db.json"
http_status=$(curl -sS -o "$HEALTH_DB_JSON" -w "%{http_code}" --max-time 5 "$API_BASE/api/health/database" || echo "000")
echo "API health/database probe HTTP status: $http_status" | tee -a "$OUT_LOG"
if [ "$http_status" != "200" ]; then
  echo "ERROR: API health/database probe returned HTTP $http_status. See $HEALTH_DB_JSON" | tee -a "$OUT_LOG" >&2
  cat "$HEALTH_DB_JSON" | tee -a "$OUT_LOG" || true
  exit 1
fi
if ! grep -q '"status"\s*:\s*"Healthy"' "$HEALTH_DB_JSON" || ! grep -q '"database"\s*:\s*"Connected"' "$HEALTH_DB_JSON"; then
  echo "ERROR: API health/database response did not indicate a connected DB. See $HEALTH_DB_JSON" | tee -a "$OUT_LOG" >&2
  cat "$HEALTH_DB_JSON" | tee -a "$OUT_LOG" || true
  exit 1
fi

# Fetch workloads
http_code=$(curl -sS -w "%{http_code}" -o "$OUT_JSON" "$API_URL" || true)
echo "GET $API_URL -> HTTP $http_code" | tee -a "$OUT_LOG"

count=0
if [ "$http_code" = "200" ]; then
  if command -v python3 >/dev/null 2>&1; then
    count=$(python3 - <<PY
import sys, json
try:
    data=json.load(open('$OUT_JSON'))
    print(len(data) if isinstance(data, list) else 0)
except Exception:
    print(0)
PY
)
  else
    # crude fallback: check for opening [
    if grep -q '^\s*\[' "$OUT_JSON" 2>/dev/null; then
      count=$(grep -o '\[' "$OUT_JSON" | wc -l || true)
    else
      count=0
    fi
  fi
fi

echo "Existing workloads count: $count" | tee -a "$OUT_LOG"

if [ "$count" -gt 0 ]; then
  echo "No create needed." | tee -a "$OUT_LOG"
  exit 0
fi

echo "No workloads found — creating one." | tee -a "$OUT_LOG"

cat > "$POST_JSON" <<'JSON'
{
  "WorkloadId": 0,
  "Name": "AutoCreated Workload",
  "Description": "Created by check_and_create_workload.sh",
  "AzureNamePrefix": "acw",
  "LandingZonesCount": 0,
  "ResourcesCount": 0,
  "PrimaryPOC": "dev@example.com",
  "SecondaryPOC": null,
  "WorkloadEnvironmentRegions": []
}
JSON

post_code=$(curl -sS -o "$LOG_DIR/api_workload_post_response.json" -w "%{http_code}" -X POST -H "Content-Type: application/json" --data-binary "@$POST_JSON" "$API_URL" || true)
echo "POST $API_URL -> HTTP $post_code" | tee -a "$OUT_LOG"
echo "Response saved to $LOG_DIR/api_workload_post_response.json" | tee -a "$OUT_LOG"

if [ "$post_code" != "201" ] && [ "$post_code" != "200" ]; then
  echo "Create failed (HTTP $post_code). See logs." | tee -a "$OUT_LOG"
  exit 2
fi

echo "Create succeeded (HTTP $post_code). Re-checking workloads..." | tee -a "$OUT_LOG"
curl -sS -o "$OUT_JSON" "$API_URL" || true
if command -v python3 >/dev/null 2>&1; then
  new_count=$(python3 - <<PY
import sys, json
try:
    data=json.load(open('$OUT_JSON'))
    print(len(data) if isinstance(data, list) else 0)
except Exception:
    print(0)
PY
)
else
  new_count=0
fi
echo "New workloads count: ${new_count:-unknown}" | tee -a "$OUT_LOG"

echo "Done. Logs: $OUT_LOG" | tee -a "$OUT_LOG"
exit 0

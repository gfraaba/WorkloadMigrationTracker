#!/usr/bin/env bash
set -euo pipefail

# restart_services.sh [WEBAPP_PORT] [API_PORT]
# Stops WebApp and WebApi if running (pidfile + port), then starts WebApi then WebApp.
# Writes logs to _docs/tests/webapi_run.log and _docs/tests/webapp_run.log and pidfiles.

# To run and *see* the script output live while also saving it to a file, use:
#
#   bash -lc "chmod +x _scripts/restart_services.sh; _scripts/restart_services.sh 5049 5005 2>&1 | tee _docs/tests/restart_services_output.log"
#
# The above shows the script's console output while tee writes the same output
# to `_docs/tests/restart_services_output.log` for later inspection.
#
# If you also want the script to stream the child service log files (WebApi + WebApp)
# into your console after starting them, set the environment variable
# `SHOW_CHILD_LOGS=1` when invoking the script. Example:
#
#   SHOW_CHILD_LOGS=1 _scripts/restart_services.sh 5049 5005
#
# This will tail -F the files `_docs/tests/webapi_run.log` and `_docs/tests/webapp_run.log`
# and keep streaming until you press Ctrl+C.

WEBAPP_PORT=${1:-5049}
API_PORT=${2:-5005}

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOG_DIR="$ROOT/_docs/tests"
mkdir -p "$LOG_DIR"

# Source .devcontainer/.env or .env to provide DB_* values when running locally.
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
fi

# If a Docker container is exposing port 1433, the DB is running in Docker and
# host processes should connect to it via localhost (port forwarded). Override
# DB_SERVER for host-run WebApi so the connection string resolves correctly.
if docker ps --format '{{.Ports}}' | grep -q '1433'; then
  echo "Detected a container exposing port 1433; overriding DB_SERVER=localhost for host-run WebApi"
  export DB_SERVER=localhost
fi

WEBAPP_DIR="$ROOT/WebApp"
WEBAPI_DIR="$ROOT/WebApi"

WEBAPP_LOG="$LOG_DIR/webapp_run.log"
WEBAPI_LOG="$LOG_DIR/webapi_run.log"
WEBAPP_PIDFILE="$LOG_DIR/webapp.pid"
WEBAPI_PIDFILE="$LOG_DIR/webapi.pid"

echo "--- restart_services: stopping existing services if any ---"

stop_pidfile() {
  pidfile="$1"
  name="$2"
  if [ -f "$pidfile" ]; then
    pid=$(cat "$pidfile" || true)
    if [ -n "$pid" ] && kill -0 "$pid" 2>/dev/null; then
      echo "Stopping $name (pid $pid) from pidfile"
      kill "$pid" || true
      sleep 1
    fi
    rm -f "$pidfile" || true
  fi
}

stop_port() {
  port="$1"
  name="$2"
  pids=$(lsof -ti tcp:"$port" || true)
  if [ -n "$pids" ]; then
    echo "Killing processes for $name on port $port: $pids"
    kill $pids || true
    sleep 1
  fi
}

stop_pidfile "$WEBAPP_PIDFILE" "WebApp"
stop_pidfile "$WEBAPI_PIDFILE" "WebApi"
stop_port "$WEBAPP_PORT" "WebApp"
stop_port "$API_PORT" "WebApi"

echo "--- restart_services: starting WebApi on port $API_PORT ---"
(
  cd "$WEBAPI_DIR"
  nohup dotnet run --urls "http://0.0.0.0:$API_PORT" > "$WEBAPI_LOG" 2>&1 &
  echo $! > "$WEBAPI_PIDFILE"
)

echo "WebApi starting (logs -> $WEBAPI_LOG), pidfile: $WEBAPI_PIDFILE"

echo "Waiting for WebApi to respond with seeded data (up to 60s)..."
webapi_started=0
for i in $(seq 1 60); do
  # Request a seeded endpoint that should return non-empty data when DB seeding ran
  body=$(curl -sS --max-time 3 "http://localhost:$API_PORT/api/resourcecategories" 2>/dev/null || true)
  http_status=$(echo "$body" | sed -n '1p' >/dev/null 2>&1 && echo "200" || echo "000")

  # Determine emptiness: an empty JSON array is '[]' (possibly with whitespace)
  if [ "$body" != "" ] && ! echo "$body" | grep -q -E '^\s*\[\s*\]\s*$'; then
    webapi_started=1
    echo "WebApi returned seeded data after ${i}s"
    break
  fi

  if grep -q -E 'Now listening on|Application started' "$WEBAPI_LOG" 2>/dev/null; then
    echo "WebApi process reported listening (probe returned ${http_status}) at ${i}s; continuing to wait"
  else
    echo "WebApi probe returned ${http_status} (empty body) at ${i}s"
  fi

  sleep 1
done
if [ $webapi_started -eq 0 ]; then
  echo "ERROR: WebApi did not return seeded data within 60s. See $WEBAPI_LOG"
  echo
  echo "Last 120 lines of $WEBAPI_LOG for debugging:"
  tail -n 120 "$WEBAPI_LOG" || true
  exit 1
fi

# Additional health/database probe to ensure DB connectivity
HEALTH_DB_JSON="$LOG_DIR/health_db.json"
http_status=$(curl -sS -o "$HEALTH_DB_JSON" -w "%{http_code}" --max-time 5 "http://localhost:$API_PORT/api/health/database" || echo "000")
echo "API health/database probe HTTP status: $http_status"
if [ "$http_status" != "200" ]; then
  echo "ERROR: API health/database probe returned HTTP $http_status. See $HEALTH_DB_JSON" >&2
  tail -n 120 "$WEBAPI_LOG" || true
  exit 1
fi
if ! grep -q '"status"\s*:\s*"Healthy"' "$HEALTH_DB_JSON" || ! grep -q '"database"\s*:\s*"Connected"' "$HEALTH_DB_JSON"; then
  echo "ERROR: API health/database response did not indicate a connected DB. See $HEALTH_DB_JSON" >&2
  tail -n 120 "$WEBAPI_LOG" || true
  exit 1
fi

echo "--- restart_services: starting WebApp on port $WEBAPP_PORT ---"
(
  cd "$WEBAPP_DIR"
  nohup dotnet run --urls "http://0.0.0.0:$WEBAPP_PORT" > "$WEBAPP_LOG" 2>&1 &
  echo $! > "$WEBAPP_PIDFILE"
)

echo "WebApp starting (logs -> $WEBAPP_LOG), pidfile: $WEBAPP_PIDFILE"

echo "Waiting for WebApp to respond (up to 60s)..."
webapp_started=0
for i in $(seq 1 60); do
  if curl -sS --max-time 1 "http://localhost:$WEBAPP_PORT/" >/dev/null 2>&1; then
    webapp_started=1
    echo "WebApp responding after ${i}s"
    break
  fi
  if grep -q -E 'Now listening on|Application started' "$WEBAPP_LOG" 2>/dev/null; then
    webapp_started=1
    echo "WebApp process reported listening after ${i}s"
    break
  fi
  sleep 1
done
if [ $webapp_started -eq 0 ]; then
  echo "Warning: WebApp did not become ready within 60s. See $WEBAPP_LOG"
fi

echo "--- final probes ---"
echo "WebApi (localhost:$API_PORT)/api/workloads:";
curl -sS -D - "http://localhost:$API_PORT/api/workloads" | sed -n '1,120p' || true

echo
echo "WebApp (localhost:$WEBAPP_PORT)/api/workloads (via SPA host):";
curl -sS -D - "http://localhost:$WEBAPP_PORT/api/workloads" | sed -n '1,120p' || true

echo
echo "WebApp root (localhost:$WEBAPP_PORT)/:";
curl -sS -D - "http://localhost:$WEBAPP_PORT/" | sed -n '1,40p' || true

echo "Restart complete. Check logs in $LOG_DIR for details."

# Optionally stream child logs (WebApi/WebApp) to console if requested.
# This is useful when you want to watch the applications' stdout in real time.
if [ "${SHOW_CHILD_LOGS:-0}" = "1" ]; then
  echo
  echo "Streaming child logs (press Ctrl+C to stop):"
  tail -F "$WEBAPI_LOG" "$WEBAPP_LOG"
fi

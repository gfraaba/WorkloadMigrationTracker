#!/usr/bin/env bash
set -euox pipefail

# wait_and_probe_webapp.sh [WEBAPP_PORT] [API_PORT]
# - WEBAPP_PORT: port where the WebApp is listening (default 5049)
# - API_PORT: port where the API listens for direct probes (default 5005)

WEBAPP_PORT=${1:-5049}
API_PORT=${2:-5005}
# Allow overriding host addresses via env vars for host vs container runs
# APP_BASE_URL example: http://localhost:5049 or override via env for container runs
# TEST_API_BASE_URL example: http://localhost:5005 or override via env for container runs
APP_BASE_URL=${APP_BASE_URL:-http://localhost:${WEBAPP_PORT}}
TEST_API_BASE_URL=${TEST_API_BASE_URL:-http://localhost:${API_PORT}}
LOG_FILE=_docs/tests/webapp_run.log

# If there's a recorded PID from a previous run, try to stop it first.
PID_FILE=_docs/tests/webapp.pid
if [ -f "$PID_FILE" ]; then
  oldpid=$(cat "$PID_FILE" || true)
  if [ -n "$oldpid" ] && kill -0 "$oldpid" 2>/dev/null; then
    echo "Killing existing WebApp process from pidfile: $oldpid"
    kill "$oldpid" || true
    sleep 1
  fi
  rm -f "$PID_FILE" || true
fi

# Also kill any process listening on the configured port (defensive)
existing_pids=$(lsof -ti tcp:"$WEBAPP_PORT" || true)
if [ -n "$existing_pids" ]; then
  echo "Killing processes listening on port $WEBAPP_PORT: $existing_pids"
  kill $existing_pids || true
  sleep 1
fi

echo "Waiting up to 60s for WebApp on port ${WEBAPP_PORT}..."
started=0
for i in $(seq 1 60); do
  if grep -q -E 'Now listening on|Application started' "$LOG_FILE" 2>/dev/null; then
    started=1
    echo "Started after ${i}s"
    break
  fi
  if curl -sS --max-time 1 "http://localhost:${WEBAPP_PORT}/" >/dev/null 2>&1; then
    started=1
    echo "Port responding after ${i}s"
    break
  fi
  sleep 1
done

if [ $started -eq 0 ]; then
  echo 'WebApp did not start within 60s'
fi

echo '--- webapp_run.log (tail 200) ---'
tail -n 200 "$LOG_FILE" || true

echo '--- probe APP_BASE_URL root ---'
curl -sS -D - "$APP_BASE_URL/" || true

echo
echo '--- probe APP_BASE_URL root (explicit host check) ---'
curl -sS -D - "${APP_BASE_URL}/" || true

echo
echo '--- probe /api/workloads via APP_BASE_URL ---'
curl -sS -D - "${APP_BASE_URL}/api/workloads" | sed -n '1,200p' || true

echo
echo '--- probe API direct at TEST_API_BASE_URL ---'
curl -sS -D - "${TEST_API_BASE_URL}/api/workloads" | sed -n '1,200p' || true

#!/bin/bash
set -eo pipefail

# Start SQL Server in background
/opt/mssql/bin/sqlservr &
PID=$!

# Wait function with timeout
wait_for_sql() {
    echo "Waiting for SQL Server to start..."
    for i in {1..60}; do
        if /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &>/dev/null; then
            return 0
        fi
        sleep 2
    done
    return 1
}

# Wait for SQL Server
if ! wait_for_sql; then
    echo "SQL Server failed to start after 2 minutes. Logs:"
    cat /var/opt/mssql/log/errorlog
    exit 1
fi

# Execute scripts
for f in $(ls /docker-entrypoint-initdb.d/*.sql | sort); do
    echo "Executing $f"
    for attempt in {1..3}; do
        if /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i "$f" -b -I; then
            break
        elif [ $attempt -eq 3 ]; then
            echo "Failed to execute $f after 3 attempts"
            exit 1
        fi
        sleep 5
    done
done

# Keep SQL Server running
wait $PID
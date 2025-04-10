#!/bin/bash
# *NOTE*: This DID NOT work! This script is used to wait for SQL Server to be ready before running any other commands.
set -e

echo "Waiting for SQL Server to be ready..."
for i in {1..60}; do
    # To get around the certificate verification error, we use the -C and -N options with sqlcmd.
    /opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$DB_PASSWORD" -Q "SELECT 1" -C -N &>/dev/null && break
    echo "SQL Server is not ready yet. Retrying in 2 seconds..."
    sleep 2
done

if [ $i -eq 60 ]; then
    echo "SQL Server did not become ready in time. Exiting."
    exit 1
fi

echo "SQL Server is ready!"
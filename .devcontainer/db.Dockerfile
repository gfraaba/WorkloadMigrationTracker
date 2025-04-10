ARG PLATFORM
FROM --platform=${PLATFORM} mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04

# Configure for ARM64 performance
ENV MSSQL_AGENT_ENABLED=false \
    MSSQL_MEMORY_LIMIT_MB=2048 \
    MSSQL_LCID=1033

# Install prerequisites with proper permissions
USER root
RUN mkdir -p /var/lib/apt/lists/partial && \
    chmod 755 /var/lib/apt/lists && \
    apt-get update && \
    apt-get install -y curl gnupg

# Configure Microsoft packages (ARM64-specific)
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /usr/share/keyrings/microsoft.gpg && \
    echo "deb [arch=arm64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" > /etc/apt/sources.list.d/mssql-release.list

# Install tools
RUN apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 unixodbc && \
    rm -rf /var/lib/apt/lists/*

# Update PATH to include mssql-tools
ENV PATH="/opt/mssql-tools18/bin:${PATH}"

# Copy initialization files and set permissions
COPY ./Database/ /database/
RUN chmod -R 750 /database && \
    chown -R mssql:root /database && \
    mkdir -p /var/opt/mssql/log && \
    chmod 770 /var/opt/mssql/log && \
    chown mssql:root /var/opt/mssql/log

USER mssql

# The base image mcr.microsoft.com/mssql/server is pre-configured to start the MSSQL server when the container runs.
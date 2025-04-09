# Official ARM64 SQL Server 2022 CU18
ARG PLATFORM
FROM --platform=${PLATFORM} mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04

# Configure for ARM64 performance
ENV MSSQL_AGENT_ENABLED=false \
    MSSQL_MEMORY_LIMIT_MB=2048 \
    MSSQL_LCID=1033

# 1. Install prerequisites with proper permissions
USER root
RUN mkdir -p /var/lib/apt/lists/partial && \
    chmod 755 /var/lib/apt/lists && \
    apt-get update && \
    apt-get install -y curl gnupg

# 2. Configure Microsoft packages (ARM64-specific)
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /usr/share/keyrings/microsoft.gpg && \
    echo "deb [arch=arm64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" > /etc/apt/sources.list.d/mssql-release.list

# 3. Install tools
RUN apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc && \
    rm -rf /var/lib/apt/lists/*

# 4. Initialize scripts
RUN mkdir -p /docker-entrypoint-initdb.d && \
    chmod 775 /docker-entrypoint-initdb.d

# COPY --chown=mssql:root ./Database/Scripts/*.sql /docker-entrypoint-initdb.d/
# RUN chmod 750 /docker-entrypoint-initdb.d/*.sql

# # 5. Add entrypoint script
# COPY ./Database/Scripts/init-db.sh /usr/local/bin/
# RUN chmod +x /usr/local/bin/init-db.sh

# Copy initialization files
COPY ./Database/Scripts/init-db.sh /usr/local/bin/
COPY ./Database/Scripts/ /docker-entrypoint-initdb.d/

RUN chmod +x /usr/local/bin/init-db.sh && \
    chmod -R 750 /docker-entrypoint-initdb.d && \
    chown -R mssql:root /docker-entrypoint-initdb.d && \
    mkdir -p /var/opt/mssql/log && \
    chown mssql:root /var/opt/mssql/log

USER mssql
CMD ["/usr/local/bin/init-db.sh"]
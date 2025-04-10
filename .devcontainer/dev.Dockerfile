# Use build arguments for flexibility; 
ARG PLATFORM

# Start from the Debian version specific latest Python base image
FROM mcr.microsoft.com/dotnet/sdk:9.0

# *NOTE: Dockerfile scope behavior—arguments (ARG) and environment variables (ENV) work in different phases of the build process.
#   Docker does not carry ARG values beyond layers unless explicitly restated.
#   ARG variables are only available during the build stage but do not persist into later layers unless explicitly reassigned.
#   ENV variables persist and are available at runtime when the container is running.
ARG PLATFORM
ENV PLATFORM=${PLATFORM}

# Install essential tools
RUN apt-get update && \
    apt-get install -y curl gnupg wget apt-transport-https software-properties-common && \
    rm -rf /var/lib/apt/lists/*

COPY . /workspace
# Navigate to the WebApi directory and add required packages
RUN cd /workspace/WebApi && \
    dotnet add package Swashbuckle.AspNetCore && \
    dotnet add package Microsoft.EntityFrameworkCore && \
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer && \
    dotnet tool install --global dotnet-ef && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> ~/.bashrc

ENV PATH="$PATH:/root/.dotnet/tools"

# RUN chmod +x /workspace/.scripts/wait-for-db.sh && \
#     /workspace/.scripts/wait-for-db.sh

RUN cd /workspace/WebApi && \
    dotnet tool restore && \
    dotnet add package Microsoft.EntityFrameworkCore.Design
    # if [ ! -d "Migrations" ] || [ -z "$(ls -A Migrations)" ]; then \
    #     ~/.dotnet/tools/dotnet-ef migrations add InitialCreate; \
    # fi && \
    # ~/.dotnet/tools/dotnet-ef database update

# Configure Microsoft packages (ARM64-specific)
RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg && \
    echo "deb [arch=arm64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/12/prod/ bookworm main" | tee /etc/apt/sources.list.d/mssql-release.list

# Install tools
RUN apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="$PATH:/opt/mssql-tools18/bin"

# Set the working directory
WORKDIR /workspace

# Add the current directory contents into the container
ADD . /workspace

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

# Set the working directory
WORKDIR /workspace

# Add the current directory contents into the container
ADD . /workspace

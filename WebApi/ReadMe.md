Building and Running the WebApi Docker Container

Based on your setup with the database container and VS Code dev container already running, here's how to manually build and run your WebApi Docker container for end-to-end testing:

Step 1: Build the WebApi Docker Image
Navigate to the root directory of your project (where the top-level folder containing both WebApi and Database folders is located) and run:

docker build -t webapi:latest -f WebApi/api.Dockerfile .

This builds the Docker image using the api.Dockerfile and tags it as "webapi:latest".

Step 2: Run the WebApi Container
Now run the container, making sure it can communicate with your existing database container:

docker run -d \
  --name webapi \
  -p 8080:8080 \
  -e DB_PASSWORD=<your-db-password> \
  -e DB_SERVER=<your-db-container-name-or-ip-or-dockerCompose_serviceName> \
  --network <your-db-container-network> \
  --network-alias api \
  webapi:latest

Replace:

<your-db-password> with the same password used in your docker-compose file
<your-db-container-network> with the Docker network your DB container is running on
If you don't know the network name, you can find it with:

docker network ls

It's likely named something like workspace_default based on your directory structure.

docker network inspect workloadmigrationtracker_devcontainer_default

docker network inspect workloadmigrationtracker_devcontainer_default --format '{{ range $i, $c := .Containers }}{{ printf "%s: %s\n" $c.Name (index $c "Name") }}{{ end }}'

docker inspect -f '{{range $key, $value := .NetworkSettings.Networks}}{{$key}}{{end}}' <your-db-container-name>

Replace <your-db-container-name> with the actual name of your running DB container (likely "db" based on your docker-compose).

Error: No such object: db
The error "No such object: db" suggests that your database container is either not running or is running with a different name than expected. Based on your Docker images, it looks like Docker Compose created images with prefixed names.

Solution Steps
1. Check running containers:
docker ps

Note the actual container name for your database (likely something with "workloadmigrationtracker_devcontainer-db" in it).

$DockerDBContainerName = docker ps --format '{{.Names}}' --filter "ancestor=workloadmigrationtracker_devcontainer-db:latest"

$DockerDBContainerId = docker ps -q --filter "ancestor=workloadmigrationtracker_devcontainer-db:latest"

2. Get the actual network name and container name:
# List networks
docker network ls

# Find which network the DB container is on
docker inspect $DockerDBContainerName --format '{{range $key, $value := .NetworkSettings.Networks}}{{$key}}{{end}}'


# List of Commands to run:
cd ~/github/repos/WorkloadMigrationTracker/
docker ps | grep db
docker build -t webapi:latest -f WebApi/api.Dockerfile .
$DB_SERVER = 'db'
$DB_NAME = 'WorkloadMigration'
$DB_USER = 'sa'
$DB_PASSWORD = 'YourStrong@Passw0rd'

$DockerDBContainerName = docker ps --format '{{.Names}}' --filter "ancestor=workloadmigrationtracker_devcontainer-db:latest"

$DockerDBContainerNetwork = docker network ls --filter name=workloadmigrationtracker --format "{{.Name}}" | grep -v "none"
$DockerDBContainerNetwork = docker inspect $DockerDBContainerName --format '{{range $key, $value := .NetworkSettings.Networks}}{{$key}}{{end}}'

docker network inspect $DockerDBContainerNetwork

docker network inspect $DockerDBContainerNetwork --format '{{ range $i, $c := .Containers }}{{ printf "%s: %s\n" $c.Name (index $c "Name") }}{{ end }}'

docker run -d --name webapi -p 8080:8080 -e DB_SERVER=$DB_SERVER -e DB_NAME=$DB_NAME -e DB_USER=$DB_USER -e DB_PASSWORD=$DB_PASSWORD --network $DockerDBContainerNetwork --network-alias api webapi:latest

docker logs webapi
docker exec -it webapi bash
  #apt-get update && apt-get install -y curl procps net-tools iputils-ping && netstat -tuln && ping -c 4 db && curl http://localhost:8080/api/health && curl curl http://localhost:8080/api/health/database
  #apt-get install -y sqlcmd && sqlcmd -S db -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT @@VERSION'

Step 3: Verify the Connection
Your WebApi should now be accessible at http://localhost:8080/api

To confirm it's running and properly connected to the database:

curl http://localhost:8080/api/health
curl http://localhost:8080/api/health/database

(Assuming your API has a health endpoint; otherwise, try another endpoint that connects to the database)

These endpoints will return JSON responses indicating the health status of your API and database connection, making it easy to verify that everything is working properly.

Troubleshooting Tips
If the container has connection issues with the database, ensure it's using the hostname "db" to connect (as specified in your docker-compose.yml)

Check container logs if you encounter problems:
docker logs webapi

docker port webapi

If your application has a specific connection string configuration, make sure it's using environment variables or is correctly configured to connect to "db" as the server name.

Troubleshooting Steps
Docker's DNS Resolution Works Differently Than Expected: When containers are on the same Docker network, they can resolve each other by container name or service name (from docker-compose), not necessarily by the actual hostname set in the container.

# Correct Commands for Network Inspection:

For docker-compose environments: Services defined in docker-compose can always reach each other by their service name (not container name). So in your docker-compose.yml, when you have:

services:
  db:
    # ... configuration

Other services can reach this by simply using "db" as the hostname, regardless of the actual container name (which might have prefixes like "workloadmigrationtracker_devcontainer-db").

Correct Approach for Manual Containers:
When running containers manually that need to connect to containers started by docker-compose:

$DockerDBContainerName = docker ps --format '{{.Names}}' --filter "ancestor=workloadmigrationtracker_devcontainer-db:latest"

$DockerDBContainerNetwork = docker network ls --filter name=workloadmigrationtracker --format "{{.Name}}" | grep -v "none"
$DockerDBContainerNetwork = docker inspect $DockerDBContainerName --format '{{range $key, $value := .NetworkSettings.Networks}}{{$key}}{{end}}'

docker network inspect $DockerDBContainerNetwork

docker network inspect $DockerDBContainerNetwork --format '{{ range $i, $c := .Containers }}{{ printf "%s: %s\n" $c.Name (index $c "Name") }}{{ end }}'

docker run -d --name webapi -p 8080:8080 -e DB_PASSWORD=$DB_PASSWORD -e DB_SERVER=$DockerDBContainerName --network $DockerDBContainerNetwork --network-alias api webapi:latest

This would allow the container to resolve 'db' automatically when connected to the same network.

Why the previous Commands (Troubleshooting Tips) Didn't Work:
The docker inspect commands were looking for a container named exactly "db", but Docker Compose likely created a container with a prefix/suffix like "workloadmigrationtracker_devcontainer-db-1".

However, on the Docker network, this container is still accessible via the service name "db" (as defined in docker-compose.yml) for DNS resolution purposes, which is why "ping -c 4 db" worked inside your container.

*** This is a subtle but important aspect of Docker networking: service name resolution works even when the actual container names are different! ***
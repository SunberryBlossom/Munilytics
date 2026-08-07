using Aspire.Hosting;
using Microsoft.Extensions.Validation;

var builder = DistributedApplication.CreateBuilder(args);

// Postgres setup
var postgresPassword = builder.AddParameter("postgres-password", "postgres");
var postgresServer = builder.AddPostgres("hydra", password: postgresPassword)
    .WithImageRegistry("ghcr.io")
    .WithImage("hydradatabase/hydra", "latest")
    .WithHostPort(port: 5433)
    .WithDataVolume("hydra-data")
    .WithPgAdmin(p =>
    {
        p.WithLifetime(ContainerLifetime.Persistent);
        p.WithVolume("pgadmin", "/var/lib/pgadmin");
    })
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = postgresServer.AddDatabase("MunilyticsDb");

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Cubejs setup
var cube = builder.AddContainer("cube", "cubejs/cube")
    .WithHttpEndpoint(port: 4000, targetPort: 4000, name: "http")
    .WithEnvironment("CUBEJS_DEV_MODE", "true")
    .WithEnvironment("CUBEJS_WEB_SOCKETS", "true")
    .WithEnvironment("CUBEJS_API_SECRET", "mysupersecret")
    // Postgres Integration
    .WithEnvironment("CUBEJS_DB_TYPE", "postgres")
    // The server resource, not the database resource — the latter is not a hostname.
    .WithEnvironment("CUBEJS_DB_HOST", postgresServer.Resource.Name)
    .WithEnvironment("CUBEJS_DB_PORT", "5432")
    .WithEnvironment("CUBEJS_DB_NAME", "MunilyticsDb")
    .WithEnvironment("CUBEJS_DB_USER", "postgres")
    .WithEnvironment("CUBEJS_DB_PASS", postgresPassword)
    // Redis Integration
    .WithEnvironment("CUBEJS_CACHE_AND_QUEUE_DRIVER", "memory")
    .WithEnvironment("CUBEJS_REDIS_URL", ReferenceExpression.Create($"redis://{redis.Resource.Name}:6379"))
    // Cubejs folder
    .WithBindMount("../Munilytics.Cube", "/cube/conf")
    .WaitFor(postgresServer)
    .WaitFor(redis);

var migrationService = builder
    .AddProject<Projects.Munilytics_MigrationService>("migrations")
    .WithReference(postgres)
    .WaitFor(postgres);

var server = builder.AddProject<Projects.Munilytics_Server>("server")
    .WithReference(postgres)
    .WithReference(redis)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WaitFor(migrationService);

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    // Vite only exposes VITE_-prefixed vars to the browser bundle.
    .WithEnvironment("VITE_CUBE_URL", cube.GetEndpoint("http"))
    .WaitFor(server)
    .WaitFor(cube);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();

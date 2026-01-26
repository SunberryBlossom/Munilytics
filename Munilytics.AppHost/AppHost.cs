var builder = DistributedApplication.CreateBuilder(args);

// Postgres setup
var postgresPassword = builder.AddParameter("postgres-password", "postgres");
var postgres = builder.AddPostgres("hydra", password: postgresPassword)
    .WithImageRegistry("ghcr.io")
    .WithImage("hydradatabase/hydra", "latest")
    .WithDataVolume("hydra-data")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("MunilyticsDb");

var server = builder.AddProject<Projects.Munilytics_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();

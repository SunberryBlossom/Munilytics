using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using JasperFx;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Infrastructure.Kolada;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.Runtime.Agents;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Get connectionstring from Aspire
var connectionString = builder.Configuration.GetConnectionString("MunilyticsDb");

// Wolverine
builder.Host.UseWolverine(opt =>
{
    opt.PersistMessagesWithPostgresql(connectionString!);
    opt.UseEntityFrameworkCoreTransactions();
    opt.Policies.UseDurableInboxOnAllListeners();
    opt.Policies.UseDurableOutboxOnAllSendingEndpoints();
    opt.Policies.OnException<ApplicationException>()
        .RetryWithCooldown(
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(15)
        );
    opt.Services.AddSingularAgent<KoladaSyncAgent>();
    opt.LocalQueue("sync-kolada")
    .UseDurableInbox()
    .Sequential();
});

// Postgres setup with wolverine
builder.Services.AddDbContextWithWolverineIntegration<MunilyticsDbContext>(o => o.UseNpgsql(connectionString));

// Auth
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<MunilyticsDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthorization();

// Fastendpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "ChasRooms API";
        s.Version = "v1";
        s.Description = "APIs for ChasRooms";

    };
});
// Registers HttpClient service with DI for our KoladaService
builder.Services.AddHttpClient<IKoladaService, KoladaService>()
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(4);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(4);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.UseFileServer();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

await app.RunJasperFxCommands(args);
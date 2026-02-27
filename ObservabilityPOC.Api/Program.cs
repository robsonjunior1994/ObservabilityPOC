using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Instrumentation.Process;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ObservabilityPOC.Api.Data;
using ObservabilityPOC.Api.Middleware;
using ObservabilityPOC.Api.Repositories;
using ObservabilityPOC.Api.Services;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 🔹 Meter para Health
// =========================
var healthMeter = new Meter("ObservabilityPOC.Health");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();

// =========================
// 🔹 Health Checks
// =========================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// =========================
// 🔹 OpenTelemetry - Logs
// =========================
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(
        ResourceBuilder.CreateDefault().AddService("ObservabilityPOC.Api"));

    logging.IncludeScopes = true;
    logging.ParseStateValues = true;
    logging.IncludeFormattedMessage = true;

    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
    });
});

// =========================
// 🔹 OpenTelemetry - Traces & Metrics
// =========================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("ObservabilityPOC.Api")
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("host.name", Environment.MachineName)
        }))
    .WithTracing(tracing => tracing
        .SetSampler(new AlwaysOnSampler())
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddSqlClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("ObservabilityPOC.Health") // 👈 MUITO IMPORTANTE
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));

var app = builder.Build();

// =========================
// 🔹 Observable Gauges
// =========================
var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

healthMeter.CreateObservableGauge<int>("app_health_status", () =>
{
    var result = healthCheckService.CheckHealthAsync().Result;
    var value = result.Status == HealthStatus.Healthy ? 1 : 0;

    return new Measurement<int>(value);
});

healthMeter.CreateObservableGauge<int>("database_health_status", () =>
{
    var result = healthCheckService.CheckHealthAsync().Result;

    var dbEntry = result.Entries.FirstOrDefault(e => e.Key == "database");

    var value = dbEntry.Value.Status == HealthStatus.Healthy ? 1 : 0;

    return new Measurement<int>(value);
});

// =========================
// 🔹 Endpoint HTTP de Health
// =========================
app.MapHealthChecks("/api/health");

// =========================
// 🔹 Middleware e pipeline
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

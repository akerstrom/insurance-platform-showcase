using System.Text.Json.Serialization;
using InsuranceMainframe.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "legacy-mainframe" }));

app.MapGet("/policies/{pid}", (string pid) =>
{
    var policies = PolicyStore.GetByPid(pid);
    return policies.Count > 0
        ? Results.Ok(policies)
        : Results.NotFound(new { error = "Not found", message = $"No policies found for PID {pid}" });
});

app.Run();

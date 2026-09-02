using Api.Extensions;
using Api.Middleware;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    if (allowedOrigins.Length == 0 || Array.TrueForAll(allowedOrigins, o => o.Contains("localhost")))
    {
        app.Logger.LogWarning(
            "Cors:AllowedOrigins is not configured with a non-localhost origin for the {Environment} environment - the deployed frontend will be rejected by CORS until this is set.",
            app.Environment.EnvironmentName);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseCors(Api.Extensions.ServiceCollectionExtensions.GetCorsPolicyName());

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program
{
}

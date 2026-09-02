using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.FunctionalTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        // The checked-in appsettings.json intentionally ships an empty Jwt:Key
        // (it's a secret) - JwtTokenGenerator throws immediately when signing
        // with a zero-length key, so tests need their own test-only value.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-only-signing-key-not-a-real-secret-0123456789"
            });
        });

        builder.ConfigureServices(services =>
        {
            // AddInfrastructureServices already wired up UseNpgsql on this
            // IServiceCollection, contributing both core EF services and
            // Npgsql-specific provider services. Strip all of it out before
            // adding Sqlite, so the two providers never coexist.
            var efDescriptors = services
                .Where(d =>
                    (d.ServiceType.FullName ?? string.Empty).StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
                    (d.ServiceType.FullName ?? string.Empty).StartsWith("Npgsql", StringComparison.Ordinal) ||
                    d.ServiceType == typeof(ApplicationDbContext))
                .ToList();

            foreach (var descriptor in efDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                options.AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
                options.UseSqlite(_connection);
            });

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<TestEmailSender>();
            services.AddSingleton<IEmailSender>(provider => provider.GetRequiredService<TestEmailSender>());
        });
    }

    public TestEmailSender EmailSender => Services.GetRequiredService<TestEmailSender>();

    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

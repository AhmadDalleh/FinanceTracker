using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.FunctionalTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

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

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

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

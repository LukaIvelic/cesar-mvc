using System.Security.Claims;
using System.Text.Encodings.Web;
using cesar.Data;
using cesar.Features.Identity;
using cesar.Features.LeadIntelligence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cesar.Tests;

public sealed class CesarWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly bool _authenticated;
    private readonly string _databaseName = $"CesarTests-{Guid.NewGuid():N}";

    public CesarWebApplicationFactory(bool authenticated = true)
    {
        _authenticated = authenticated;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=cesar_tests",
                ["Authentication:Google:ClientId"] = "test-client-id",
                ["Authentication:Google:ClientSecret"] = "test-client-secret"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(AppDbContext)
                    || descriptor.ServiceType == typeof(DbContextOptions)
                    || descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || descriptor.ServiceType.Name.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<ILeadIntelligenceAnalyzer>();
            services.AddSingleton<ILeadIntelligenceAnalyzer, TestLeadIntelligenceAnalyzer>();

            services.AddSingleton(new TestAuthState(_authenticated));
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }
}

public sealed class TestLeadIntelligenceAnalyzer : ILeadIntelligenceAnalyzer
{
    public Task<LeadIntelligenceAnalysisResult> AnalyzeAsync(
        string rawJsonData,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LeadIntelligenceAnalysisResult
        {
            FamiliarityIndex = 0.81,
            DataDensityScore = 0.73
        });
}

public sealed record TestAuthState(bool Authenticated);

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    private readonly TestAuthState _authState;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestAuthState authState)
        : base(options, logger, encoder)
    {
        _authState = authState;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_authState.Authenticated)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.Role, AppRoles.Admin),
            new Claim(ClaimTypes.Role, AppRoles.Manager)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

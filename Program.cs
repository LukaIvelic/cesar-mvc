using cesar.Data;
using cesar.Extensions;
using cesar.Features.DesignTemplates.Entities;
using cesar.Features.JsonKeyStats.Entities;
using cesar.Features.LeadIntelligence.Entities;
using cesar.Features.RawLead.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Intentionally not used because existing data already exists.
    // await SeedLabDataAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task SeedLabDataAsync(AppDbContext dbContext)
{
    if (await dbContext.RawLeads.AnyAsync() || await dbContext.DesignTemplates.AnyAsync())
    {
        return;
    }

    var now = DateTime.UtcNow;

    var rawLeads = Enumerable.Range(1, 12)
        .Select(i => new RawLead
        {
            SourceSystem = i % 2 == 0 ? "web_form" : "crm_sync",
            ExternalId = $"LEAD-{1000 + i}",
            RawJsonData = $$"""
            {
              "fullName": "Lead {{i}}",
              "email": "lead{{i}}@example.com",
              "company": "Company {{i}}",
              "score": {{i * 10}}
            }
            """,
            IngestedAt = now.AddMinutes(-i * 8),
            ValidFrom = now.AddMinutes(-i * 8),
            ValidTo = null
        })
        .ToList();

    // filter (.Where) + sort desc (.OrderByDescending) + map (.Select) + materialize (.ToList)
    var leadPreview = rawLeads
        .Where(l => l.SourceSystem == "web_form")
        .OrderByDescending(l => l.IngestedAt)
        .Select(l => new { l.ExternalId, l.SourceSystem, l.IngestedAt })
        .ToList();

    _ = leadPreview.Count;

    await dbContext.RawLeads.AddRangeAsync(rawLeads);

    var templates = new List<DesignTemplate>
    {
        new()
        {
            Name = "Simple Email",
            ContentType = ContentType.Mail,
            HtmlMarkup = "<h1>Hello {{fullName}}</h1><p>Company: {{company}}</p><p>Score: {{score}}</p>",
            PlaceholderSchema = """{"fullName":"","company":"","score":0}""",
            ValidFrom = now,
            ValidTo = null
        },
        new()
        {
            Name = "Landing Snippet",
            ContentType = ContentType.HTML,
            HtmlMarkup = "<section><strong>{{fullName}}</strong> from {{company}}</section>",
            PlaceholderSchema = """{"fullName":"","company":""}""",
            ValidFrom = now,
            ValidTo = null
        },
        new()
        {
            Name = "SMS Template",
            ContentType = ContentType.SMS,
            HtmlMarkup = "Hi {{fullName}}, thanks for your interest in {{company}}.",
            PlaceholderSchema = """{"fullName":"","company":""}""",
            ValidFrom = now,
            ValidTo = null
        }
    };

    await dbContext.DesignTemplates.AddRangeAsync(templates);

    var keyStats = new List<JsonKeyStat>
    {
        new() { Key = "fullName", Occurrences = rawLeads.Count, ValidFrom = now, ValidTo = null },
        new() { Key = "email", Occurrences = rawLeads.Count, ValidFrom = now, ValidTo = null },
        new() { Key = "company", Occurrences = rawLeads.Count, ValidFrom = now, ValidTo = null },
        new() { Key = "score", Occurrences = rawLeads.Count, ValidFrom = now, ValidTo = null }
    };

    await dbContext.JsonKeyStats.AddRangeAsync(keyStats);

    var intelligence = rawLeads
        .Select(l => new LeadIntelligence
        {
            LeadId = l.Id,
            ContentHash = $"hash-{Guid.NewGuid():N}",
            FamiliarityIndex = Math.Round(Random.Shared.NextDouble() * 100, 2),
            DataDensityScore = Math.Round(Random.Shared.NextDouble() * 100, 2),
            LastAnalyzedAt = now,
            ValidFrom = now,
            ValidTo = null
        })
        .ToList();

    await dbContext.LeadIntelligences.AddRangeAsync(intelligence);

    await dbContext.SaveChangesAsync();

    // Required query shape: Where/filter + OrderByDescending + Select/map + ToListAsync
    var persistedLeadProjection = await dbContext.RawLeads
        .Where(l => l.ValidTo == null)
        .OrderByDescending(l => l.IngestedAt)
        .Select(l => new { l.Id, l.ExternalId, l.SourceSystem })
        .ToListAsync();

    _ = persistedLeadProjection.Count;
}

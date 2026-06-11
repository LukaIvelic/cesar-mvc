using cesar.Features.JsonKeyStats.Entities;
using cesar.Features.LeadIntelligence.Entities;
using cesar.Features.RawLead.Entities;
using cesar.Features.DesignTemplates.Entities;
using cesar.Features.Identity.Entities;
using cesar.Features.Weather.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cesar.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    public DbSet<RawLead> RawLeads { get; set; }
    public DbSet<LeadIntelligence> LeadIntelligences { get; set; }
    public DbSet<JsonKeyStat> JsonKeyStats { get; set; }
    public DbSet<DesignTemplate> DesignTemplates { get; set; }
    public DbSet<DesignTemplateAttachment> DesignTemplateAttachments { get; set; }
}

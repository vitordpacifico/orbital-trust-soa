using Microsoft.EntityFrameworkCore;
using OrbitalTrust.Api.Domain.Entities;

namespace OrbitalTrust.Api.Infrastructure.Data;

/// <summary>
/// PERSISTÊNCIA — DbContext do EF Core (SQLite). Mapeia só as entidades escalares.
/// </summary>
public class OrbitalTrustDbContext : DbContext
{
    public OrbitalTrustDbContext(DbContextOptions<OrbitalTrustDbContext> options) : base(options)
    {
    }

    public DbSet<LeituraAmbiental> Leituras => Set<LeituraAmbiental>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
}

using Microsoft.EntityFrameworkCore;
using OrbitalTrust.Api.Domain.Entities;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Infrastructure.Data.Repositories;

/// <summary>
/// REPOSITÓRIO — implementa IAlertaRepository sobre o EF Core. Registrado como scoped via DI.
/// </summary>
public class AlertaRepository : IAlertaRepository
{
    private readonly OrbitalTrustDbContext _db;

    public AlertaRepository(OrbitalTrustDbContext db)
    {
        _db = db;
    }

    public async Task<int> Adicionar(Alerta alerta)
    {
        _db.Alertas.Add(alerta);
        await _db.SaveChangesAsync();
        return alerta.Id;
    }

    public async Task<IEnumerable<Alerta>> ListarOrdenadoPorData()
    {
        return await _db.Alertas
            .OrderByDescending(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<Alerta?> ObterPorId(int id)
    {
        return await _db.Alertas.FindAsync(id);
    }
}

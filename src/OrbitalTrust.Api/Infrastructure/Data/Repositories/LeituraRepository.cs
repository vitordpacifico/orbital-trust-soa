using Microsoft.EntityFrameworkCore;
using OrbitalTrust.Api.Domain.Entities;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Infrastructure.Data.Repositories;

/// <summary>
/// REPOSITÓRIO — implementa ILeituraRepository sobre o EF Core. Registrado como scoped via DI.
/// </summary>
public class LeituraRepository : ILeituraRepository
{
    private readonly OrbitalTrustDbContext _db;

    public LeituraRepository(OrbitalTrustDbContext db)
    {
        _db = db;
    }

    public async Task<int> Adicionar(LeituraAmbiental leitura)
    {
        _db.Leituras.Add(leitura);
        await _db.SaveChangesAsync();
        return leitura.Id;
    }

    public async Task<IEnumerable<LeituraAmbiental>> ListarOrdenadoPorData()
    {
        return await _db.Leituras
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<LeituraAmbiental?> ObterPorId(int id)
    {
        return await _db.Leituras.FindAsync(id);
    }
}

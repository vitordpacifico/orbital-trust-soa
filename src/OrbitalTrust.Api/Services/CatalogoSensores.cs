using OrbitalTrust.Api.Domain.Entities;

namespace OrbitalTrust.Api.Services;

/// <summary>
/// Catálogo em memória dos sensores disponíveis (um óptico + um térmico), para o endpoint
/// GET /api/sensores demonstrar os tipos. Registrado como singleton via DI.
/// A lista é polimórfica: guarda SensorBase, com instâncias dos dois subtipos.
/// </summary>
public class CatalogoSensores
{
    public IReadOnlyList<SensorBase> Sensores { get; }

    public CatalogoSensores()
    {
        Sensores = new List<SensorBase>
        {
            new SensorOptico  { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Nome = "Sentinel-2 Óptico", Online = true },
            new SensorTermico { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Nome = "Landsat Térmico",   Online = true }
        };
    }
}

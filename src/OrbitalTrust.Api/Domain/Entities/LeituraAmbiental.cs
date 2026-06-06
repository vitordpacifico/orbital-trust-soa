using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// ENTIDADE DE PERSISTÊNCIA (mapeada no EF Core) — só tipos escalares.
/// A coordenada NUNCA é gravada em claro: guardamos só CoordenadaCriptografada (base64).
/// </summary>
public class LeituraAmbiental
{
    public int Id { get; set; }                          // PK auto
    public Guid SensorId { get; set; }
    public TipoSensor TipoSensor { get; set; }
    public DateTime DataHora { get; set; }               // DATETIME
    public double CoberturaNuvem { get; set; }
    public double QualidadeImagem { get; set; }
    public double ConfiancaAnalise { get; set; }
    public double RiscoEstimado { get; set; }
    public TipoEventoAmbiental TipoEvento { get; set; }
    public string CoordenadaCriptografada { get; set; } = string.Empty; // AES-256-GCM em base64
    public double IcoValor { get; set; }
    public NivelRisco NivelRisco { get; set; }
}

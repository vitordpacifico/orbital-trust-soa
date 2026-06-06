using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// ENTIDADE DE PERSISTÊNCIA (mapeada no EF Core) — alerta emitido a partir de uma leitura.
/// Também guarda a coordenada apenas criptografada.
/// </summary>
public class Alerta
{
    public int Id { get; set; }                          // PK auto
    public int LeituraId { get; set; }
    public DateTime DataHora { get; set; }               // DATETIME
    public TipoEventoAmbiental TipoEvento { get; set; }
    public NivelRisco NivelRisco { get; set; }
    public double IcoValor { get; set; }
    public string IcoCategoria { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string CoordenadaCriptografada { get; set; } = string.Empty; // AES-256-GCM em base64
}

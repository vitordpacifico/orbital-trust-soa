using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.DTOs;

/// <summary>
/// DTO DE ENTRADA — separa o contrato da API do modelo de domínio.
/// </summary>
public class LeituraInputDTO
{
    public Guid SensorId { get; set; }
    public TipoSensor TipoSensor { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CoberturaNuvem { get; set; }   // 0–1
    public double QualidadeImagem { get; set; }  // 0–1
    public double ConfiancaAnalise { get; set; } // 0–1
    public double RiscoEstimado { get; set; }    // 0–1
    public TipoEventoAmbiental TipoEvento { get; set; }
    public DateTime? DataHora { get; set; }      // opcional; se null, usa UtcNow
}

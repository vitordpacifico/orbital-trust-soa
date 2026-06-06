using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.DTOs;

/// <summary>
/// DTO DE SAÍDA — a coordenada aqui vem DESCRIPTOGRAFADA (prova que o decrypt funciona).
/// </summary>
public class AlertaOutputDTO
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public TipoEventoAmbiental TipoEvento { get; set; }
    public NivelRisco NivelRisco { get; set; }
    public double Ico { get; set; }
    public string IcoCategoria { get; set; } = string.Empty;
    public double Latitude { get; set; }   // descriptografada
    public double Longitude { get; set; }  // descriptografada
    public string Mensagem { get; set; } = string.Empty;
}

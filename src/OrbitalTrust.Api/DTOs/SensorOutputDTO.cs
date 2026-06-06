using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.DTOs;

/// <summary>
/// DTO DE SAÍDA — representa um sensor disponível na plataforma.
/// </summary>
public class SensorOutputDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoSensor Tipo { get; set; }
    public bool Online { get; set; }
}

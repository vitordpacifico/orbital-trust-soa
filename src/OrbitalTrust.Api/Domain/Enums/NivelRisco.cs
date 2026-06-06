namespace OrbitalTrust.Api.Domain.Enums;

/// <summary>
/// Nível de risco final do evento, decidido cruzando o risco estimado (ML) com o ICO.
/// </summary>
public enum NivelRisco
{
    Baixo,
    Moderado,
    Alto,
    Critico
}

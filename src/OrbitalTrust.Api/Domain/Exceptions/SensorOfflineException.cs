namespace OrbitalTrust.Api.Domain.Exceptions;

/// <summary>
/// EXCEÇÃO CUSTOMIZADA — sensor marcado como offline não pode produzir leitura
/// confiável. Mapeada para 503 Service Unavailable no middleware global.
/// </summary>
public class SensorOfflineException : Exception
{
    public SensorOfflineException(string mensagem) : base(mensagem)
    {
    }
}
